import { Book } from './book.model';
import { Client } from './client.model';

export interface Loan {
    id: number;
    bookId: number;
    book?: Book;
    clientId: number;
    client?: Client;
    loanDate?: string;
    returnDate?: string | null;
}
