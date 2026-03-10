import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'iban',
  standalone: true
})
export class IbanPipe implements PipeTransform {
  transform(value: string | null | undefined): string {
    // 1. If the IBAN is empty or null, just return 'N/A'
    if (!value) return 'N/A';

    // 2. Strip out any existing spaces just to be safe
    const cleanIban = value.replace(/\s+/g, '');

    // 3. Use a regular expression to inject a space after every 4th character
    return cleanIban.replace(/(.{4})/g, '$1 ').trim();
  }
}
