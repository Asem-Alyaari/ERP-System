// ============================================================
// PrimeNG v20 Imports Barrel
// Based on actual available packages in node_modules/primeng
// Note: 'dropdown' was removed in v17+ → use 'select' instead
// ============================================================

import { TableModule }           from 'primeng/table';
import { DialogModule }          from 'primeng/dialog';
import { ButtonModule }          from 'primeng/button';
import { InputTextModule }       from 'primeng/inputtext';
import { ToastModule }           from 'primeng/toast';
import { ConfirmDialogModule }   from 'primeng/confirmdialog';
import { TagModule }             from 'primeng/tag';
import { ToolbarModule }         from 'primeng/toolbar';
import { TooltipModule }         from 'primeng/tooltip';
import { CardModule }            from 'primeng/card';
import { BadgeModule }           from 'primeng/badge';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { DividerModule }         from 'primeng/divider';
import { SelectModule }          from 'primeng/select';        // replaces DropdownModule
import { InputNumberModule }     from 'primeng/inputnumber';
import { CheckboxModule }        from 'primeng/checkbox';
import { RadioButtonModule }     from 'primeng/radiobutton';
import { PanelModule }           from 'primeng/panel';
import { MenuModule }            from 'primeng/menu';
import { AvatarModule }          from 'primeng/avatar';
import { SkeletonModule }        from 'primeng/skeleton';
import { MessageModule }         from 'primeng/message';
import { DatePickerModule }      from 'primeng/datepicker';    // replaces CalendarModule
import { TextareaModule }        from 'primeng/textarea';
import { MultiSelectModule }     from 'primeng/multiselect';
import { PaginatorModule }       from 'primeng/paginator';
import { DrawerModule }          from 'primeng/drawer';        // replaces SidebarModule
import { PopoverModule }         from 'primeng/popover';       // replaces OverlayPanelModule
import { AccordionModule }       from 'primeng/accordion';
import { ToggleSwitchModule }    from 'primeng/toggleswitch';  // replaces InputSwitchModule
import { DataViewModule }        from 'primeng/dataview';
import { ChipModule }            from 'primeng/chip';
import { StepsModule }           from 'primeng/steps';
import { TabsModule }            from 'primeng/tabs';
import { TreeModule }            from 'primeng/tree';           // for StockGroups tree view

export const PRIMENG_IMPORTS = [
  // Data Display
  TableModule,
  DataViewModule,
  PaginatorModule,

  // Overlay / Feedback
  DialogModule,
  ToastModule,
  ConfirmDialogModule,
  MessageModule,
  DrawerModule,
  PopoverModule,

  // Buttons & Actions
  ButtonModule,
  MenuModule,
  AccordionModule,
  StepsModule,
  TabsModule,

  // Form Inputs
  InputTextModule,
  InputNumberModule,
  TextareaModule,
  SelectModule,
  MultiSelectModule,
  CheckboxModule,
  RadioButtonModule,
  ToggleSwitchModule,
  DatePickerModule,

  // Display / Layout
  TagModule,
  BadgeModule,
  CardModule,
  PanelModule,
  DividerModule,
  AvatarModule,
  SkeletonModule,
  ProgressSpinnerModule,
  ChipModule,

  // Layout
  ToolbarModule,

  // Misc
  TooltipModule,
  TreeModule,
] as const;
