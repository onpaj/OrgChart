import {
  OrgChartResponse,
  Position,
  Employee,
  CreatePositionRequest,
  UpdatePositionRequest,
  CreateEmployeeRequest,
  UpdateEmployeeRequest,
  GraphUserInfo,
  UserPhotoResponse,
} from '../types/orgchart';
import { getAccessToken } from './auth';

const API_BASE_URL = process.env.REACT_APP_API_URL || '/api';

/**
 * API client for organizational chart data
 */
export class OrgChartApi {
  private baseUrl: string;

  constructor(baseUrl: string = API_BASE_URL) {
    this.baseUrl = baseUrl;
  }

  /**
   * Creates headers with authentication token if available
   */
  private async createHeaders(additionalHeaders: Record<string, string> = {}): Promise<HeadersInit> {
    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
      ...additionalHeaders,
    };

    // Always try to get access token (both development and production)
    const token = await getAccessToken();
    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
      console.log('Added Authorization header with token');
    } else {
      console.log('No token available for Authorization header');
    }

    return headers;
  }

  /**
   * Fetches organizational structure from the backend API
   */
  async getOrganizationStructure(): Promise<OrgChartResponse> {
    const response = await fetch(`${this.baseUrl}/orgchart`, {
      method: 'GET',
      headers: await this.createHeaders(),
      credentials: 'include',
    });

    if (!response.ok) {
      throw new Error(`Failed to fetch organization structure: ${response.status} ${response.statusText}`);
    }

    return response.json();
  }

  /**
   * Creates a new position
   */
  async createPosition(request: CreatePositionRequest): Promise<Position> {
    const response = await fetch(`${this.baseUrl}/orgchart/positions`, {
      method: 'POST',
      headers: await this.createHeaders(),
      credentials: 'include',
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: response.statusText }));
      throw new Error(`Failed to create position: ${error.message || response.statusText}`);
    }

    return response.json();
  }

  /**
   * Updates an existing position
   */
  async updatePosition(id: string, request: UpdatePositionRequest): Promise<Position> {
    const response = await fetch(`${this.baseUrl}/orgchart/positions/${id}`, {
      method: 'PUT',
      headers: await this.createHeaders(),
      credentials: 'include',
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: response.statusText }));
      throw new Error(`Failed to update position: ${error.message || response.statusText}`);
    }

    return response.json();
  }

  /**
   * Deletes a position
   */
  async deletePosition(id: string): Promise<void> {
    const response = await fetch(`${this.baseUrl}/orgchart/positions/${id}`, {
      method: 'DELETE',
      headers: await this.createHeaders(),
      credentials: 'include',
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: response.statusText }));
      throw new Error(`Failed to delete position: ${error.message || response.statusText}`);
    }
  }

  /**
   * Creates a new employee
   */
  async createEmployee(request: CreateEmployeeRequest): Promise<Employee> {
    const response = await fetch(`${this.baseUrl}/orgchart/employees`, {
      method: 'POST',
      headers: await this.createHeaders(),
      credentials: 'include',
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: response.statusText }));
      throw new Error(`Failed to create employee: ${error.message || response.statusText}`);
    }

    return response.json();
  }

  /**
   * Updates an existing employee
   */
  async updateEmployee(id: string, request: UpdateEmployeeRequest): Promise<Employee> {
    const response = await fetch(`${this.baseUrl}/orgchart/employees/${id}`, {
      method: 'PUT',
      headers: await this.createHeaders(),
      credentials: 'include',
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: response.statusText }));
      throw new Error(`Failed to update employee: ${error.message || response.statusText}`);
    }

    return response.json();
  }

  /**
   * Deletes an employee
   */
  async deleteEmployee(id: string): Promise<void> {
    const response = await fetch(`${this.baseUrl}/orgchart/employees/${id}`, {
      method: 'DELETE',
      headers: await this.createHeaders(),
      credentials: 'include',
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: response.statusText }));
      throw new Error(`Failed to delete employee: ${error.message || response.statusText}`);
    }
  }

  /**
   * Gets user profile information from Microsoft Graph by email
   */
  async getUserProfile(email: string): Promise<GraphUserInfo> {
    const response = await fetch(`${this.baseUrl}/user/profile?email=${encodeURIComponent(email)}`, {
      method: 'GET',
      headers: await this.createHeaders(),
      credentials: 'include',
    });

    if (!response.ok) {
      throw new Error(`Failed to fetch user profile: ${response.status} ${response.statusText}`);
    }

    return response.json();
  }

  /**
   * Gets user profile photo from Microsoft Graph by email
   */
  async getUserPhoto(email: string): Promise<UserPhotoResponse> {
    const response = await fetch(`${this.baseUrl}/user/photo?email=${encodeURIComponent(email)}`, {
      method: 'GET',
      headers: await this.createHeaders(),
      credentials: 'include',
    });

    if (!response.ok) {
      throw new Error(`Failed to fetch user photo: ${response.status} ${response.statusText}`);
    }

    return response.json();
  }

  /**
   * Gets multiple user profiles by email addresses (batch operation)
   */
  async getUserProfilesBatch(emails: string[]): Promise<Record<string, GraphUserInfo | null>> {
    const response = await fetch(`${this.baseUrl}/user/profiles/batch`, {
      method: 'POST',
      headers: await this.createHeaders(),
      credentials: 'include',
      body: JSON.stringify(emails),
    });

    if (!response.ok) {
      throw new Error(`Failed to fetch user profiles: ${response.status} ${response.statusText}`);
    }

    return response.json();
  }
}

// Default API client instance
export const orgChartApi = new OrgChartApi();