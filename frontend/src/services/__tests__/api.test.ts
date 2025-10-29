import { OrgChartApi } from '../api';
import { OrgChartResponse } from '../../types/orgchart';

// Mock fetch
global.fetch = jest.fn();

// Mock the auth module
jest.mock('../auth', () => ({
  getAccessToken: jest.fn().mockResolvedValue(null),
}));

describe('OrgChartApi', () => {
  let api: OrgChartApi;

  beforeEach(() => {
    api = new OrgChartApi('/test-api');
    jest.clearAllMocks();
  });

  describe('getOrganizationStructure', () => {
    test('should fetch organization structure successfully', async () => {
      const mockResponse: OrgChartResponse = {
        organization: {
          name: 'Test Company',
          positions: [
            {
              id: 'pos1',
              title: 'CEO',
              description: 'Chief Executive Officer',
              department: 'Executive',
              level: 1,
              employees: [
                {
                  id: 'emp1',
                  name: 'John Doe',
                  email: 'john@example.com',
                  startDate: '2020-01-01',
                },
              ],
            },
          ],
        },
        permissions: {
          canEdit: false,
        },
      };

      (fetch as jest.Mock).mockResolvedValueOnce({
        ok: true,
        json: jest.fn().mockResolvedValueOnce(mockResponse),
      });

      const result = await api.getOrganizationStructure();

      expect(fetch).toHaveBeenCalledWith('/test-api/orgchart', {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
      });

      expect(result).toEqual(mockResponse);
    });

    test('should throw error when fetch fails', async () => {
      (fetch as jest.Mock).mockResolvedValueOnce({
        ok: false,
        status: 500,
        statusText: 'Internal Server Error',
      });

      await expect(api.getOrganizationStructure()).rejects.toThrow(
        'Failed to fetch organization structure: 500 Internal Server Error'
      );
    });

    test('should use correct API endpoint', async () => {
      (fetch as jest.Mock).mockResolvedValueOnce({
        ok: true,
        json: jest.fn().mockResolvedValueOnce({
          organization: { name: 'Test', positions: [] },
          permissions: { canEdit: false },
        }),
      });

      await api.getOrganizationStructure();

      expect(fetch).toHaveBeenCalledWith('/test-api/orgchart', expect.objectContaining({
        method: 'GET',
        credentials: 'include',
      }));
    });

    test('should handle network errors', async () => {
      (fetch as jest.Mock).mockRejectedValueOnce(new Error('Network error'));

      await expect(api.getOrganizationStructure()).rejects.toThrow('Network error');
    });
  });

  describe('constructor', () => {
    test('should use default API base URL when not provided', () => {
      const defaultApi = new OrgChartApi();
      expect(defaultApi).toBeInstanceOf(OrgChartApi);
    });

    test('should use custom base URL when provided', () => {
      const customApi = new OrgChartApi('/custom-api');
      expect(customApi).toBeInstanceOf(OrgChartApi);
    });
  });
});