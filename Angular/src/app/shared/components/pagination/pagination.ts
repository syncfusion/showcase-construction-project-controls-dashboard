import { Component, Input, Output, EventEmitter } from '@angular/core';
import { IconComponent } from '../icon';

@Component({
  selector: 'app-pagination',
  imports: [IconComponent],
  templateUrl: './pagination.html',
  styleUrl: './pagination.css',
})
export class Pagination {
  @Input({ required: true }) page!: number;
  @Input({ required: true }) pageSize!: number;
  @Input({ required: true }) totalCount!: number;
  @Output() pageChange = new EventEmitter<number>();

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount / this.pageSize));
  }
}
