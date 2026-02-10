import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ClientService } from '../../services/client.service';
import { Client } from '../../models/client.model';
import { TranslateModule } from '@ngx-translate/core';
import { ClientForm } from '../client-form/client-form';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { ConfirmDialogComponent } from '../confirm-dialog/confirm-dialog';

@Component({
  selector: 'app-clients-list',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule, MatDialogModule],
  templateUrl: './clients-list.html',
  styleUrl: './clients-list.css'
})
export class ClientsList implements OnInit {
  clients: Client[] = [];

  constructor(
    private clientService: ClientService,
    private dialog: MatDialog
  ) { }

  ngOnInit(): void {
    this.loadClients();
  }

  loadClients(): void {
    this.clientService.getClients().subscribe(data => {
      this.clients = data;
    });
  }

  deleteClient(id: number): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: {
        title: 'Delete User',
        message: 'Are you sure you want to delete this user?'
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.clientService.deleteClient(id).subscribe(() => {
          this.loadClients();
        });
      }
    });
  }

  openModal(id: number = 0): void {
    const dialogRef = this.dialog.open(ClientForm, {
      width: '600px',
      data: { id: id }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadClients();
      }
    });
  }
}
