import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';


@Component({
  selector: 'app-item-list',
  imports: [CommonModule, MatCardModule, MatButtonModule],
  templateUrl: './item-list.html',
  styleUrls: ['./item-list.css']
})

export class ItemListComponent {
  viewMode: 'grid' | 'list' = 'grid';
  items = [
    { name: 'カメラ', image: 'assets/camera.jpg' },
    { name: '本', image: 'assets/book.jpg' }
  ];
}
