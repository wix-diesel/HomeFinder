import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-admin',
  imports: [CommonModule, MatCardModule, MatButtonModule],
  templateUrl: './admin.html',
  styleUrl: './admin.css'
})
export class Admin {

}

export class AdminComponent {
  users = [
    { name: '山田太郎', email: 'taro@example.com' },
    { name: '鈴木花子', email: 'hanako@example.com' }
  ];
}
