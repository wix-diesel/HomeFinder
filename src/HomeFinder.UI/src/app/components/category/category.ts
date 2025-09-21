import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-category',
  imports: [CommonModule, MatCardModule, MatButtonModule],
  templateUrl: './category.html',
  styleUrls: ['./category.css']
})
export class Category {

}

export class CategoryComponent {
  categories = [
    { name: '電子機器', description: '最新のガジェットやデバイス' },
    { name: '書籍', description: '多様なジャンルの本' }
  ];
}