import { TestBed } from '@angular/core/testing';

import { PrimeChackHistoryService } from './prime-chack-history.service';

describe('PrimeCheckHistoryService', () => {
  let service: PrimeChackHistoryService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PrimeChackHistoryService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
