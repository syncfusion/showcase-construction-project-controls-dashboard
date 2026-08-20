import {
  Component,
  ElementRef,
  HostListener,
  Input,
  Output,
  EventEmitter,
  SimpleChanges,
  ViewChild,
  OnChanges,
} from '@angular/core';
import { IconComponent } from '../icon';

@Component({
  selector: 'app-modal',
  imports: [IconComponent],
  templateUrl: './modal.html',
  styleUrl: './modal.css',
})
export class Modal implements OnChanges {
  @Input() open = false;
  @Input() title = '';
  @Input() subtitle?: string;
  @Input() size: 'md' | 'lg' | 'xl' = 'md';
  @Input() hasFooter = false;
  @Output() closed = new EventEmitter<void>();

  @ViewChild('panel') panelRef?: ElementRef<HTMLDivElement>;

  private previouslyFocused: HTMLElement | null = null;

  ngOnChanges(changes: SimpleChanges): void {
    if (!changes['open']) return;
    if (this.open) {
      this.previouslyFocused = document.activeElement as HTMLElement | null;
      document.body.style.overflow = 'hidden';
      // Wait a tick so the panel is in the DOM before focusing it.
      setTimeout(() => this.panelRef?.nativeElement.focus());
    } else if (changes['open'].previousValue) {
      document.body.style.overflow = '';
      this.previouslyFocused?.focus();
    }
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.open) this.closed.emit();
  }

  onOverlayMouseDown(): void {
    this.closed.emit();
  }

  onPanelMouseDown(event: MouseEvent): void {
    event.stopPropagation();
  }
}
