import { TestBed } from '@angular/core/testing';

import { BankApi } from './bank-api';

describe('BankApi', () => {
  let service: BankApi;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(BankApi);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
