// Type definitions for organizational chart data

export interface Employee {
  id: string;
  name: string;
  email: string;
  startDate: string;
  url?: string;
}

export interface Position {
  id: string;
  title: string;
  description: string;
  level?: number; // Calculated internally, not from API
  parentPositionId?: string;
  department: string;
  employees: Employee[];
  url?: string;
}

export interface OrganizationData {
  name: string;
  positions: Position[];
}

export interface UserPermissions {
  canEdit: boolean;
}

export interface OrgChartResponse {
  organization: OrganizationData;
  permissions: UserPermissions;
}

export interface PositionRect {
  id: string;
  x: number;
  y: number;
  width: number;
  height: number;
}

// Request types for CUD operations
export interface CreatePositionRequest {
  title: string;
  description: string;
  parentPositionId?: string;
  department: string;
  url?: string;
}

export interface UpdatePositionRequest {
  title: string;
  description: string;
  parentPositionId?: string;
  department: string;
  url?: string;
}

export interface CreateEmployeeRequest {
  name: string;
  email: string;
  startDate: string;
  url?: string;
  positionId: string;
}

export interface UpdateEmployeeRequest {
  name: string;
  email: string;
  startDate: string;
  url?: string;
  positionId: string;
}