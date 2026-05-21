// ============================================================
// SHARED_IMPORTS — single import array for all standalone components
//
// Usage in any component:
//   imports: [...SHARED_IMPORTS]
//
// Providers (MessageService, ConfirmationService) must still be declared
// per-component OR provided at root level in app.config.ts
// ============================================================

import { CommonModule }    from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule }    from '@angular/router';
import { PRIMENG_IMPORTS } from './primeng.imports';

export const SHARED_IMPORTS = [
  // Angular Core
  CommonModule,
  FormsModule,
  ReactiveFormsModule,
  RouterModule,

  // All PrimeNG modules
  ...PRIMENG_IMPORTS,
] as const;
