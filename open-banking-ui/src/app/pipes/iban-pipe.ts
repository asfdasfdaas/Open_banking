import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'iban',
  standalone: true
})
export class IbanPipe implements PipeTransform {
  transform(value: string | null | undefined): string {
    // If the IBAN is empty or null, just return 'N/A'
    if (!value) return 'N/A';

    // Strip out any existing spaces just to be safe
    const cleanIban = value.replace(/\s+/g, '');

    // Use a regular expression to inject a space after every 4th character
    return cleanIban.replace(/(.{4})/g, '$1 ').trim();
  }
}
