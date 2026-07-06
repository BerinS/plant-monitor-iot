import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { LucideAngularModule, Mail, Eye, EyeOff, Save, RotateCcw } from 'lucide-angular';
import { ToastrService } from 'ngx-toastr';

import { SettingsService } from '../../services/settings.service';
import { Setting } from '../../models/setting.model';
import { EventLogViewerComponent } from '../../components/event-log-viewer/event-log-viewer.component';

type FieldType = 'text' | 'number' | 'email' | 'password' | 'toggle';

interface FieldMeta {
  key: string;
  label: string;
  type: FieldType;
  placeholder?: string;
  hint?: string;
}

// Curated metadata for the known setting keys (order = render order).
// Any key returned by the API that is not listed here falls back to a plain
// text field (see fields()), so the page never silently drops a setting.
const KNOWN_FIELDS: FieldMeta[] = [
  { key: 'mail_enabled', label: 'Enable email notifications', type: 'toggle',
    hint: 'When off, the system will not send any notification emails.' },
  { key: 'mail_host', label: 'SMTP host', type: 'text', placeholder: 'smtp.resend.com' },
  { key: 'mail_port', label: 'SMTP port', type: 'number', placeholder: '465' },
  { key: 'mail_username', label: 'SMTP username', type: 'text', placeholder: 'resend' },
  { key: 'mail_password', label: 'SMTP password', type: 'password', placeholder: '••••••••',
    hint: 'Your SMTP provider API key or password.' },
  { key: 'mail_from_address', label: 'From address', type: 'email', placeholder: 'you@yourdomain.com' },
  { key: 'mail_from_name', label: 'From name', type: 'text', placeholder: 'Plant Monitor' },
  { key: 'mail_to_address', label: 'Recipient address', type: 'email', placeholder: 'you@example.com' },
];

@Component({
  selector: 'app-settings',
  imports: [FormsModule, LucideAngularModule, DatePipe, EventLogViewerComponent],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.scss',
})
export class SettingsComponent {
  readonly MailIcon = Mail;
  readonly EyeIcon = Eye;
  readonly EyeOffIcon = EyeOff;
  readonly SaveIcon = Save;
  readonly ResetIcon = RotateCcw;

  private settingsService = inject(SettingsService);
  private toastr = inject(ToastrService);

  loading = signal(true);
  saving = signal(false);
  revealPassword = signal(false);

  // The pristine values as loaded from the server (used for dirty check + reset).
  private original = signal<Record<string, string>>({});
  // The working copy edited in the form.
  formValues = signal<Record<string, string>>({});
  // Raw settings kept for the "last updated" timestamp.
  private settings = signal<Setting[]>([]);

  constructor() {
    this.load();
  }

  // Known fields whose key was actually returned by the API, followed by any
  // unrecognised keys rendered as generic text inputs.
  fields = computed<FieldMeta[]>(() => {
    const present = this.formValues();
    const known = KNOWN_FIELDS.filter(f => f.key in present);
    const knownKeys = new Set(KNOWN_FIELDS.map(f => f.key));

    const fallback: FieldMeta[] = Object.keys(present)
      .filter(k => !knownKeys.has(k))
      .map(k => ({ key: k, label: this.humanize(k), type: 'text' as FieldType }));

    return [...known, ...fallback];
  });

  mailEnabled = computed(() => this.formValues()['mail_enabled'] === 'true');

  isDirty = computed(() =>
    JSON.stringify(this.formValues()) !== JSON.stringify(this.original())
  );

  lastUpdated = computed<string | null>(() => {
    const dates = this.settings()
      .map(s => s.updatedAt)
      .filter(Boolean)
      .sort();
    return dates.length ? dates[dates.length - 1] : null;
  });

  private load() {
    this.loading.set(true);
    this.settingsService.getSettings().subscribe({
      next: (data) => {
        this.settings.set(data);
        const map: Record<string, string> = {};
        for (const s of data) {
          map[s.key] = s.value ?? '';
        }
        this.original.set(map);
        this.formValues.set({ ...map });
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load settings. Error:', err);
        this.loading.set(false);
        this.toastr.error('Could not load settings.', 'Error');
      },
    });
  }

  updateValue(key: string, value: string) {
    this.formValues.update(current => ({ ...current, [key]: value }));
  }

  toggle(key: string) {
    const next = this.formValues()[key] === 'true' ? 'false' : 'true';
    this.updateValue(key, next);
  }

  reset() {
    this.formValues.set({ ...this.original() });
  }

  save() {
    if (!this.isDirty() || this.saving()) return;

    this.saving.set(true);
    this.settingsService.updateSettings(this.formValues()).subscribe({
      next: (res) => {
        this.saving.set(false);
        // Adopt the saved values as the new pristine baseline.
        this.original.set({ ...this.formValues() });
        // Refresh timestamps so "last updated" reflects the save.
        this.load();
        this.toastr.success(res.message || 'Settings saved.', 'Success');
      },
      error: (err) => {
        console.error('Failed to save settings. Error:', err);
        this.saving.set(false);
        this.toastr.error(err?.error?.message || 'Could not save settings.', 'Error');
      },
    });
  }

  // A field (other than the enable toggle) is only editable when mail is enabled.
  isFieldDisabled(field: FieldMeta): boolean {
    return field.key !== 'mail_enabled' && !this.mailEnabled();
  }

  private humanize(key: string): string {
    return key
      .replace(/_/g, ' ')
      .replace(/\b\w/g, c => c.toUpperCase());
  }
}
