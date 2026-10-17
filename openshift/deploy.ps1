<#
.SYNOPSIS
    Builds and deploys the Library app to the current OpenShift project.

.DESCRIPTION
    The cluster has no internal image registry, so images are built in the cluster and
    pushed to an external registry (e.g. ghcr.io), from which the pods pull them.

.EXAMPLE
    $ad = (Get-Content LibraryBackend/appsettings.json -Raw | ConvertFrom-Json).AzureAd
    ./openshift/deploy.ps1 -ClientId $ad.ClientId -TenantId $ad.TenantId `
        -Registry ghcr.io/<github-user> -RegistryUser <github-user> -RegistryToken $env:GHCR_TOKEN
#>
param(
    [Parameter(Mandatory)] [string] $ClientId,
    [Parameter(Mandatory)] [string] $TenantId,
    # Registry + namespace, lowercase, e.g. ghcr.io/<github-user>
    [Parameter(Mandatory)] [string] $Registry,
    [Parameter(Mandatory)] [string] $RegistryUser,
    [Parameter(Mandatory)] [string] $RegistryToken,
    [ValidateSet('all', 'backend', 'frontend')] [string] $Only = 'all'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$Registry = $Registry.TrimEnd('/').ToLower()
$registryServer = $Registry.Split('/')[0]

function Invoke-Oc {
    & oc @args
    if ($LASTEXITCODE -ne 0) { throw "oc $($args -join ' ') failed" }
}

function Set-Secret([string] $name, [string[]] $createArgs) {
    $yaml = & oc create secret @createArgs $name --dry-run=client -o yaml
    if ($LASTEXITCODE -ne 0) { throw "creating secret $name failed" }
    $yaml | oc apply -f -
    if ($LASTEXITCODE -ne 0) { throw "applying secret $name failed" }
    Invoke-Oc label secret $name app=gutenberg-test --overwrite
}

Write-Host "Project:  $(oc project -q)" -ForegroundColor Cyan
Write-Host "Registry: $Registry" -ForegroundColor Cyan

Set-Secret gutenberg-registry @('docker-registry',
    "--docker-server=$registryServer",
    "--docker-username=$RegistryUser",
    "--docker-password=$RegistryToken")

Set-Secret gutenberg-backend-config @('generic',
    "--from-literal=AzureAd__TenantId=$TenantId",
    "--from-literal=AzureAd__ClientId=$ClientId")

function Apply-Manifest([string] $file) {
    # These manifests use a harmless placeholder so the directory can be applied
    # directly; deploy.ps1 substitutes the actual external registry.
    $content = (Get-Content $file -Raw).Replace('image-registry-placeholder', $Registry)
    $content | oc apply -f -
    if ($LASTEXITCODE -ne 0) { throw "oc apply failed for $file" }
}

# Apply foundational resources first; create the Route before deriving the Entra
# redirect URI from its generated host.
$manifestDir = $PSScriptRoot
foreach ($file in @('00-buildconfigs.yaml', '01-storage.yaml', '02-backend.yaml',
                    '03-frontend.yaml', '04-services.yaml', '05-route.yaml',
                    '06-networkpolicy.yaml')) {
    Apply-Manifest (Join-Path $manifestDir $file)
}

$routeHost = oc get route --field-selector metadata.name=gutenberg -o jsonpath='{.items[*].spec.host}'
if (-not $routeHost) {
    throw 'Route gutenberg was applied but has no host yet; check OpenShift router/domain configuration.'
}
$redirectUri = "https://$routeHost/books"

# Build arguments are stored on the frontend BuildConfig because binary builds
# ignore build arguments passed to `oc start-build`.
$frontendBuildConfig = Get-Content (Join-Path $manifestDir '00-buildconfigs.yaml') -Raw
$frontendBuildConfig = $frontendBuildConfig.Replace('image-registry-placeholder', $Registry)
$frontendBuildConfig = $frontendBuildConfig.Replace('PLACEHOLDER_CLIENT_ID', $ClientId)
$frontendBuildConfig = $frontendBuildConfig.Replace('PLACEHOLDER_TENANT_ID', $TenantId)
$frontendBuildConfig = $frontendBuildConfig.Replace('PLACEHOLDER_REDIRECT_URI', $redirectUri)
$frontendBuildConfig | oc apply -f -
if ($LASTEXITCODE -ne 0) { throw 'applying frontend BuildConfig parameters failed' }

# Upload only the build inputs (no node_modules/bin/obj) as the build context.
function Start-Build([string] $name, [string] $dir) {
    $archive = Join-Path ([IO.Path]::GetTempPath()) "$name.tar"
    tar -cf $archive --exclude=node_modules --exclude=dist --exclude=.angular `
        --exclude=bin --exclude=obj --exclude='*.db' -C $dir .
    if ($LASTEXITCODE -ne 0) { throw "tar failed for $dir" }
    try {
        Invoke-Oc start-build $name "--from-archive=$archive" --follow --wait
    } finally {
        Remove-Item $archive -ErrorAction SilentlyContinue
    }
}

if ($Only -in 'all', 'backend') {
    Start-Build gutenberg-backend "$root/LibraryBackend"
    Invoke-Oc rollout restart deployment/gutenberg-backend
}
if ($Only -in 'all', 'frontend') {
    Start-Build gutenberg-frontend "$root/LibraryFrontend"
    Invoke-Oc rollout restart deployment/gutenberg-frontend
}

Invoke-Oc rollout status deployment/gutenberg-backend --timeout=180s
Invoke-Oc rollout status deployment/gutenberg-frontend --timeout=180s

Write-Host ""
Write-Host "App:          https://$routeHost" -ForegroundColor Green
Write-Host "Redirect URI: $redirectUri  (must be registered as a SPA redirect URI in Entra ID)" -ForegroundColor Yellow
