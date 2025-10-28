import { OrgChartResponse } from '../types/orgchart';

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
   * Fetches organizational structure from the backend API
   */
  async getOrganizationStructure(): Promise<OrgChartResponse> {
    const response = await fetch(`${this.baseUrl}/orgchart`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error(`Failed to fetch organization structure: ${response.status} ${response.statusText}`);
    }

    return response.json();
  }
}

// Default API client instance
export const orgChartApi = new OrgChartApi();