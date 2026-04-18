import { Component, ElementRef, viewChild, input, effect, output } from '@angular/core';

@Component({
  selector: 'app-modal',
  standalone: true,
  templateUrl: './modal.component.html',
  styleUrl: './modal.component.scss'
})
export class ModalComponent {
  isOpen = input.required<boolean>();
  title = input<string>('Modal Title');
  closed = output<void>();
  type = input<'default' | 'danger' | 'info'>('default');
  dialog = viewChild<ElementRef<HTMLDialogElement>>('dialogElement');

  constructor() {
    effect(() => {
      const dialogEl = this.dialog()?.nativeElement;
      if (!dialogEl) return;

      if (this.isOpen()) {
        dialogEl.showModal();
      } else {        
        if (dialogEl.open) {
          dialogEl.close();
        }
      }
    });
  }

  onClose(event?: Event) {
    // for esc key
    if (event) {
      event.preventDefault(); 
    }

    const dialogEl = this.dialog()?.nativeElement;
    if (!dialogEl) return;

    // trigger exit anim
    dialogEl.classList.add('closing');
    
    const handleTransitionEnd = () => {
      dialogEl.classList.remove('closing');
      this.closed.emit(); 
    };

    dialogEl.addEventListener('transitionend', handleTransitionEnd, { once: true });
  }
}