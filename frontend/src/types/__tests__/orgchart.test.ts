import { Employee, Position, OrganizationData, OrgChartResponse } from '../orgchart';

describe('Type Definitions', () => {
  describe('Employee', () => {
    test('should have required properties', () => {
      const employee: Employee = {
        id: 'emp1',
        name: 'John Doe',
        email: 'john@example.com',
        startDate: '2020-01-01',
        isPrimary: true,
      };

      expect(employee.id).toBe('emp1');
      expect(employee.name).toBe('John Doe');
      expect(employee.email).toBe('john@example.com');
      expect(employee.startDate).toBe('2020-01-01');
      expect(employee.isPrimary).toBe(true);
    });

    test('should allow optional url property', () => {
      const employee: Employee = {
        id: 'emp1',
        name: 'John Doe',
        email: 'john@example.com',
        startDate: '2020-01-01',
        isPrimary: true,
        url: 'https://example.com/profile',
      };

      expect(employee.url).toBe('https://example.com/profile');
    });
  });

  describe('Position', () => {
    test('should have required properties', () => {
      const position: Position = {
        id: 'pos1',
        title: 'CEO',
        description: 'Chief Executive Officer',
        department: 'Executive',
        employees: [],
      };

      expect(position.id).toBe('pos1');
      expect(position.title).toBe('CEO');
      expect(position.description).toBe('Chief Executive Officer');
      expect(position.department).toBe('Executive');
      expect(position.employees).toEqual([]);
    });

    test('should allow optional properties', () => {
      const position: Position = {
        id: 'pos1',
        title: 'CEO',
        description: 'Chief Executive Officer',
        level: 1,
        parentPositionId: 'parent1',
        department: 'Executive',
        employees: [],
        url: 'https://example.com/position',
      };

      expect(position.level).toBe(1);
      expect(position.parentPositionId).toBe('parent1');
      expect(position.url).toBe('https://example.com/position');
    });
  });

  describe('OrganizationData', () => {
    test('should have name and positions', () => {
      const orgData: OrganizationData = {
        name: 'Test Company',
        positions: [],
      };

      expect(orgData.name).toBe('Test Company');
      expect(orgData.positions).toEqual([]);
    });
  });

  describe('OrgChartResponse', () => {
    test('should contain organization data', () => {
      const response: OrgChartResponse = {
        organization: {
          name: 'Test Company',
          positions: [],
        },
      };

      expect(response.organization).toBeDefined();
      expect(response.organization.name).toBe('Test Company');
    });
  });
});