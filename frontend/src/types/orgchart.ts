// Type definitions for organizational chart data

export interface Employee {
  id: string;
  name: string;
  email: string;
  startDate: string;
  isPrimary: boolean;
  url?: string;
}

export interface Position {
  id: string;
  title: string;
  description: string;
  level?: number;
  parentPositionId?: string;
  department: string;
  employees: Employee[];
  url?: string;
}

export interface OrganizationData {
  name: string;
  positions: Position[];
}

export interface OrgChartResponse {
  organization: OrganizationData;
}

export interface PositionRect {
  id: string;
  x: number;
  y: number;
  width: number;
  height: number;
}