import { type ReactNode } from 'react';
import {
  LayoutDashboard,
  Briefcase,
  Wallet,
  AlertTriangle,
  BarChart3,
  Menu,
  Bell,
  Building2,
  Sun,
  Moon,
  X,
  ChevronLeft,
  ChevronRight,
  Receipt,
  ArrowUpRight,
  ArrowDownRight,
  FileWarning,
  Plus,
  Search,
  Download,
  ArrowLeft,
  Clock,
  CheckCircle,
  FilePlus,
  ShieldAlert,
  ExternalLink,
  type LucideProps,
} from 'lucide-react';

const ICONS = {
  'layout-dashboard': LayoutDashboard,
  briefcase: Briefcase,
  wallet: Wallet,
  'alert-triangle': AlertTriangle,
  'bar-chart-3': BarChart3,
  menu: Menu,
  bell: Bell,
  'building-2': Building2,
  sun: Sun,
  moon: Moon,
  x: X,
  'chevron-left': ChevronLeft,
  'chevron-right': ChevronRight,
  receipt: Receipt,
  'arrow-up-right': ArrowUpRight,
  'arrow-down-right': ArrowDownRight,
  'file-warning': FileWarning,
  plus: Plus,
  search: Search,
  download: Download,
  'arrow-left': ArrowLeft,
  clock: Clock,
  'check-circle': CheckCircle,
  'file-plus': FilePlus,
  'shield-alert': ShieldAlert,
  'external-link': ExternalLink,
} as const satisfies Record<string, (props: LucideProps) => ReactNode>;

export type IconName = keyof typeof ICONS;

export interface IconProps extends Omit<LucideProps, 'ref'> {
  name: string;
}

export function Icon({ name, ...rest }: IconProps): ReactNode {
  const Component = (ICONS as Record<string, (props: LucideProps) => ReactNode>)[name];
  if (!Component) return null;
  return <Component aria-hidden="true" {...rest} />;
}
