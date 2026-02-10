export interface Book {
    id: number;
    title: string;
    author: string;
    isbn: string;
    isAvailable: boolean;
    categoryId?: number;
    category?: { id: number, name: string };
}
