import React, { useEffect, useState, useRef, useMemo } from 'react';
import {
  useOrgChart,
  useCreatePosition,
  useUpdatePosition,
  useDeletePosition,
  useCreateEmployee,
  useUpdateEmployee,
  useDeleteEmployee,
} from '../services/hooks';
import {
  Position,
  OrganizationData,
  PositionRect,
  Employee,
  CreatePositionRequest,
  UpdatePositionRequest,
  CreateEmployeeRequest,
  UpdateEmployeeRequest,
} from '../types/orgchart';
import PositionModal from './PositionModal';
import EmployeeModal from './EmployeeModal';
import ConfirmDialog from './ConfirmDialog';
import UserAvatar from './common/UserAvatar';
import UserDetailModal from './UserDetailModal';

const OrgChart: React.FC = () => {
  // Fetch organization data from backend
  const { data: orgChartResponse, isLoading, error: queryError } = useOrgChart();

  // Mutations
  const createPosition = useCreatePosition();
  const updatePosition = useUpdatePosition();
  const deletePosition = useDeletePosition();
  const createEmployee = useCreateEmployee();
  const updateEmployee = useUpdateEmployee();
  const deleteEmployee = useDeleteEmployee();

  const [filters, setFilters] = useState({
    department: 'all',
  });
  const [positionRects, setPositionRects] = useState<PositionRect[]>([]);
  const [zoom, setZoom] = useState(1);
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const [expandedNodes, setExpandedNodes] = useState<Set<string>>(new Set());
  const containerRef = useRef<HTMLDivElement>(null);

  // Edit mode state
  const [isEditMode, setIsEditMode] = useState(false);
  const [positionModalOpen, setPositionModalOpen] = useState(false);
  const [positionModalMode, setPositionModalMode] = useState<'create' | 'edit'>('create');
  const [editingPosition, setEditingPosition] = useState<Position | null>(null);
  const [employeeModalOpen, setEmployeeModalOpen] = useState(false);
  const [employeeModalMode, setEmployeeModalMode] = useState<'create' | 'edit'>('create');
  const [editingEmployee, setEditingEmployee] = useState<Employee | null>(null);
  const [currentPositionId, setCurrentPositionId] = useState<string>('');
  const [confirmDialogOpen, setConfirmDialogOpen] = useState(false);
  const [confirmDialogConfig, setConfirmDialogConfig] = useState<{
    title: string;
    message: string;
    onConfirm: () => void;
  }>({ title: '', message: '', onConfirm: () => {} });
  const [userDetailModalOpen, setUserDetailModalOpen] = useState(false);
  const [selectedEmployee, setSelectedEmployee] = useState<Employee | null>(null);

  // Check if user can edit
  const canEdit = orgChartResponse?.permissions?.canEdit || false;

  // Handler functions for CUD operations
  const handleCreatePosition = (parentPositionId?: string) => {
    setPositionModalMode('create');
    setEditingPosition(null);
    setCurrentPositionId(parentPositionId || '');
    setPositionModalOpen(true);
  };

  const handleEditPosition = (position: Position) => {
    setPositionModalMode('edit');
    setEditingPosition(position);
    setPositionModalOpen(true);
  };

  const handleDeletePosition = (position: Position) => {
    setConfirmDialogConfig({
      title: 'Delete Position',
      message: `Are you sure you want to delete the position "${position.title}"? This action cannot be undone.`,
      onConfirm: () => {
        deletePosition.mutate(position.id, {
          onSuccess: () => {
            setConfirmDialogOpen(false);
          },
        });
      },
    });
    setConfirmDialogOpen(true);
  };

  const handleSavePosition = (data: CreatePositionRequest | UpdatePositionRequest) => {
    if (positionModalMode === 'create') {
      createPosition.mutate(data as CreatePositionRequest, {
        onSuccess: () => {
          setPositionModalOpen(false);
        },
      });
    } else {
      updatePosition.mutate(
        { id: editingPosition!.id, request: data as UpdatePositionRequest },
        {
          onSuccess: () => {
            setPositionModalOpen(false);
          },
        }
      );
    }
  };

  const handleCreateEmployee = (positionId: string) => {
    setEmployeeModalMode('create');
    setEditingEmployee(null);
    setCurrentPositionId(positionId);
    setEmployeeModalOpen(true);
  };

  const handleEditEmployee = (employee: Employee, positionId: string) => {
    setEmployeeModalMode('edit');
    setEditingEmployee(employee);
    setCurrentPositionId(positionId);
    setEmployeeModalOpen(true);
  };

  const handleDeleteEmployee = (employee: Employee) => {
    setConfirmDialogConfig({
      title: 'Delete Employee',
      message: `Are you sure you want to delete employee "${employee.name}"? This action cannot be undone.`,
      onConfirm: () => {
        deleteEmployee.mutate(employee.id, {
          onSuccess: () => {
            setConfirmDialogOpen(false);
          },
        });
      },
    });
    setConfirmDialogOpen(true);
  };

  const handleSaveEmployee = (data: CreateEmployeeRequest | UpdateEmployeeRequest) => {
    if (employeeModalMode === 'create') {
      createEmployee.mutate(data as CreateEmployeeRequest, {
        onSuccess: () => {
          setEmployeeModalOpen(false);
        },
      });
    } else {
      updateEmployee.mutate(
        { id: editingEmployee!.id, request: data as UpdateEmployeeRequest },
        {
          onSuccess: () => {
            setEmployeeModalOpen(false);
          },
        }
      );
    }
  };

  const handleViewUserDetail = (employee: Employee) => {
    setSelectedEmployee(employee);
    setUserDetailModalOpen(true);
  };

  const toggleNode = (positionId: string) => {
    setExpandedNodes((prev) => {
      const newSet = new Set(prev);
      if (newSet.has(positionId)) {
        newSet.delete(positionId);
      } else {
        newSet.add(positionId);
      }
      return newSet;
    });
  };

  // Calculate correct level based on parent hierarchy
  const calculateLevels = (data: OrganizationData): OrganizationData => {
    const positionMap = new Map(data.positions.map((p) => [p.id, p]));

    const getLevel = (positionId: string, visited = new Set<string>()): number => {
      if (visited.has(positionId)) {
        console.error(`Circular dependency detected for position ${positionId}`);
        return 1;
      }

      const position = positionMap.get(positionId);
      if (!position) return 1;
      if (!position.parentPositionId) return 1;

      visited.add(positionId);
      return getLevel(position.parentPositionId, visited) + 1;
    };

    const updatedPositions = data.positions.map((position) => ({
      ...position,
      level: getLevel(position.id),
    })) as Position[];

    return {
      ...data,
      positions: updatedPositions,
    };
  };

  // Transform backend response to local format and calculate levels
  const orgData = useMemo(() => {
    if (!orgChartResponse) return null;

    const transformedData: OrganizationData = {
      name: orgChartResponse.organization?.name || '',
      positions: orgChartResponse.organization?.positions || [],
    };

    return calculateLevels(transformedData);
  }, [orgChartResponse]);

  // Helper function to get element position relative to scaled container
  const getElementPosition = (element: HTMLElement): { x: number; y: number } => {
    let x = 0;
    let y = 0;
    let currentElement: HTMLElement | null = element;

    // Walk up the DOM tree and accumulate offsets until we reach the scaled container
    while (currentElement && currentElement !== containerRef.current) {
      x += currentElement.offsetLeft;
      y += currentElement.offsetTop;
      currentElement = currentElement.offsetParent as HTMLElement | null;
    }

    return { x, y };
  };

  // Calculate position rectangles after render
  useEffect(() => {
    if (!containerRef.current) return;

    const updateRects = () => {
      if (!containerRef.current) return;

      const rects: PositionRect[] = [];
      const cards = containerRef.current.querySelectorAll('[data-position-id]');

      // Don't update if no cards are rendered yet
      if (cards.length === 0) return;

      cards.forEach((card) => {
        const positionId = card.getAttribute('data-position-id');
        if (!positionId) return;

        const element = card as HTMLElement;

        // Get position in pre-transform coordinates (before zoom)
        const pos = getElementPosition(element);
        const x = pos.x + element.offsetWidth / 2;
        const y = pos.y;
        const width = element.offsetWidth;
        const height = element.offsetHeight;

        rects.push({
          id: positionId,
          x,
          y,
          width,
          height,
        });
      });

      setPositionRects(rects);
    };

    // Multiple strategies to ensure layout is complete:

    // 1. Immediate update with double RAF
    requestAnimationFrame(() => {
      requestAnimationFrame(() => {
        updateRects();
      });
    });

    // 2. Delayed update as fallback (after fonts and layout settle)
    const timeoutId = setTimeout(() => {
      updateRects();
    }, 100);

    // 3. Listen for window load event
    const handleLoad = () => {
      updateRects();
    };

    if (document.readyState !== 'complete') {
      window.addEventListener('load', handleLoad);
    }

    // 4. Use ResizeObserver to detect when cards finish laying out
    const resizeObserver = new ResizeObserver(() => {
      updateRects();
    });

    // Observe the container
    if (containerRef.current) {
      resizeObserver.observe(containerRef.current);
    }

    return () => {
      clearTimeout(timeoutId);
      window.removeEventListener('load', handleLoad);
      resizeObserver.disconnect();
    };
  }, [orgData, filters, zoom]);

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-screen">
        <div className="text-xl text-indigo-600">Loading organizational structure...</div>
      </div>
    );
  }

  if (queryError) {
    return (
      <div className="flex items-center justify-center h-screen">
        <div className="text-xl text-red-600">❌ Failed to load data: {(queryError as Error).message}</div>
      </div>
    );
  }

  if (!orgData) {
    return (
      <div className="flex items-center justify-center h-screen">
        <div className="text-xl text-indigo-600">Loading organizational structure...</div>
      </div>
    );
  }

  const departments = Array.from(new Set(orgData.positions.map((p) => p.department)));

  // Helper function to recursively find all parent positions
  const getAllParentPositionIds = (positionId: string, allPositions: Position[]): Set<string> => {
    const parentIds = new Set<string>();
    const positionMap = new Map(allPositions.map((p) => [p.id, p]));

    const findParents = (id: string) => {
      const position = positionMap.get(id);
      if (position && position.parentPositionId) {
        parentIds.add(position.parentPositionId);
        findParents(position.parentPositionId);
      }
    };

    findParents(positionId);
    return parentIds;
  };

  // Filter positions based on department
  const filteredPositions = (() => {
    const allPositions = orgData.positions;

    // First, find positions that match the department filter
    let matchingPositions = allPositions;

    if (filters.department !== 'all') {
      // Find all positions in the selected department
      const departmentPositions = allPositions.filter(pos => pos.department === filters.department);

      // Collect all parent position IDs for these department positions
      const parentPositionIds = new Set<string>();
      departmentPositions.forEach(pos => {
        const parents = getAllParentPositionIds(pos.id, allPositions);
        parents.forEach(id => parentPositionIds.add(id));
      });

      // Include department positions + all their parents
      matchingPositions = allPositions.filter(pos =>
        pos.department === filters.department || parentPositionIds.has(pos.id)
      );
    }

    return matchingPositions;
  })();

  const totalEmployees = filteredPositions.reduce((sum, pos) => sum + (pos.employees?.length || 0), 0);

  const getLevelColor = (level: number) => {
    switch (level) {
      case 1:
        return 'border-l-4 border-red-500';
      case 2:
        return 'border-l-4 border-orange-500';
      case 3:
        return 'border-l-4 border-yellow-500';
      case 4:
        return 'border-l-4 border-green-500';
      default:
        return 'border-l-4 border-gray-500';
    }
  };

  // Build hierarchical tree structure
  const buildTree = (positions: Position[]): Position[] => {
    const positionMap = new Map(positions.map((p) => [p.id, p]));
    const roots: Position[] = [];

    positions.forEach((position) => {
      if (!position.parentPositionId || !positionMap.has(position.parentPositionId)) {
        roots.push(position);
      }
    });

    return roots;
  };

  const getChildren = (parentId: string): Position[] => {
    return filteredPositions.filter((p) => p.parentPositionId === parentId);
  };

  // Render compact mobile view
  const renderCompactPosition = (position: Position, depth: number = 0): JSX.Element => {
    const children = getChildren(position.id);
    const isExpanded = expandedNodes.has(position.id);
    const hasChildren = children.length > 0;

    return (
      <div key={position.id} className="mb-1">
        <div
          className="bg-white rounded-md shadow-sm p-2 border-l-2 border-indigo-500"
          style={{ marginLeft: `${depth * 12}px` }}
        >
          <div className="flex items-start gap-2">
            <div className="flex-1 min-w-0">
              <div className="flex items-center gap-1.5 mb-1">
                {hasChildren && (
                  <button
                    onClick={() => toggleNode(position.id)}
                    className="flex-shrink-0 p-0.5 hover:bg-gray-100 rounded transition-colors"
                    aria-label={isExpanded ? 'Collapse' : 'Expand'}
                  >
                    <svg
                      className="w-3.5 h-3.5 text-gray-600 transition-transform"
                      style={{ transform: isExpanded ? 'rotate(90deg)' : 'rotate(0deg)' }}
                      fill="none"
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth="2"
                      viewBox="0 0 24 24"
                      stroke="currentColor"
                    >
                      <path d="M9 5l7 7-7 7" />
                    </svg>
                  </button>
                )}
                <span className="text-xs bg-blue-100 text-blue-700 px-1.5 py-0.5 rounded font-medium flex-shrink-0">
                  {position.department}
                </span>
                {!isEditMode && (position.employees?.length || 0) > 1 && (
                  <div className="bg-indigo-600 text-white w-5 h-5 rounded-full flex items-center justify-center text-xs font-bold flex-shrink-0">
                    {position.employees?.length}
                  </div>
                )}
                {isEditMode && canEdit && (
                  <div className="flex gap-1 ml-auto">
                    <button
                      onClick={() => handleCreatePosition(position.id)}
                      className="bg-green-500 text-white p-1 rounded hover:bg-green-600 transition-colors"
                      title="Add child position"
                    >
                      <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                      </svg>
                    </button>
                    <button
                      onClick={() => handleEditPosition(position)}
                      className="bg-blue-500 text-white p-1 rounded hover:bg-blue-600 transition-colors"
                      title="Edit position"
                    >
                      <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z" />
                      </svg>
                    </button>
                    <button
                      onClick={() => handleDeletePosition(position)}
                      className="bg-red-500 text-white p-1 rounded hover:bg-red-600 transition-colors"
                      title="Delete position"
                    >
                      <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                      </svg>
                    </button>
                  </div>
                )}
              </div>
              <h3 className="font-bold text-gray-900 text-sm leading-tight mb-0.5">
                {position.url ? (
                  <a
                    href={position.url}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="hover:text-indigo-600 transition-colors"
                  >
                    {position.title}
                  </a>
                ) : (
                  position.title
                )}
              </h3>
              {position.employees && position.employees.length > 0 && (
                <div className="space-y-0.5 mt-1">
                  {position.employees.map((emp) => (
                    <div key={emp.id} className="flex items-center gap-1 group">
                      <UserAvatar 
                        email={emp.email}
                        userName={emp.name}
                        size="sm" 
                        showName={true}
                        className="flex-1 min-w-0"
                        clickable={true}
                        onClick={() => handleViewUserDetail(emp)}
                      />
                      {emp.url && (
                        <a
                          href={emp.url}
                          target="_blank"
                          rel="noopener noreferrer"
                          className="text-gray-400 hover:text-indigo-600 transition-colors"
                          title="View profile"
                        >
                          <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14" />
                          </svg>
                        </a>
                      )}
                      {isEditMode && canEdit && (
                        <div className="flex gap-0.5 opacity-0 group-hover:opacity-100 transition-opacity">
                          <button
                            onClick={() => handleEditEmployee(emp, position.id)}
                            className="bg-blue-500 text-white p-0.5 rounded hover:bg-blue-600 transition-colors"
                            title="Edit"
                          >
                            <svg className="w-2.5 h-2.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z" />
                            </svg>
                          </button>
                          <button
                            onClick={() => handleDeleteEmployee(emp)}
                            className="bg-red-500 text-white p-0.5 rounded hover:bg-red-600 transition-colors"
                            title="Delete"
                          >
                            <svg className="w-2.5 h-2.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                            </svg>
                          </button>
                        </div>
                      )}
                    </div>
                  ))}
                </div>
              )}
              {isEditMode && canEdit && (
                <button
                  onClick={() => handleCreateEmployee(position.id)}
                  className="w-full mt-1 py-1 px-2 bg-green-50 text-green-700 border border-green-200 rounded text-xs font-semibold hover:bg-green-100 transition-colors flex items-center justify-center gap-1"
                >
                  <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                  </svg>
                  Add Employee
                </button>
              )}
            </div>
          </div>
        </div>
        {hasChildren && isExpanded && (
          <div className="mt-0.5">
            {children.map((child) => renderCompactPosition(child, depth + 1))}
          </div>
        )}
      </div>
    );
  };

  // Render position card
  const renderPositionCard = (position: Position): JSX.Element => {
    const children = getChildren(position.id);

    return (
      <div key={position.id} className="flex flex-col items-center">
        {/* Position card */}
        <div
          data-position-id={position.id}
          className={`bg-white rounded-xl shadow-lg p-4 sm:p-5 md:p-6 w-full max-w-[calc(100vw-2rem)] sm:max-w-sm md:w-80 transition-all hover:shadow-2xl hover:-translate-y-1 ${getLevelColor(
            position.level ?? 1
          )} relative mb-12 sm:mb-16 md:mb-20`}
        >
          {isEditMode && canEdit && (
            <div className="absolute top-3 right-3 flex gap-1">
              <button
                onClick={() => handleCreatePosition(position.id)}
                className="bg-green-500 text-white p-1.5 rounded-full hover:bg-green-600 transition-colors"
                title="Add child position"
              >
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                </svg>
              </button>
              <button
                onClick={() => handleEditPosition(position)}
                className="bg-blue-500 text-white p-1.5 rounded-full hover:bg-blue-600 transition-colors"
                title="Edit position"
              >
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z" />
                </svg>
              </button>
              <button
                onClick={() => handleDeletePosition(position)}
                className="bg-red-500 text-white p-1.5 rounded-full hover:bg-red-600 transition-colors"
                title="Delete position"
              >
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                </svg>
              </button>
            </div>
          )}

          {!isEditMode && (position.employees?.length || 0) > 1 && (
            <div className="absolute top-3 right-3 bg-indigo-600 text-white w-7 h-7 rounded-full flex items-center justify-center text-xs font-bold">
              {position.employees?.length}
            </div>
          )}

          <div className="mb-2 sm:mb-3">
            {position.url ? (
              <a
                href={position.url}
                target="_blank"
                rel="noopener noreferrer"
                className="text-base sm:text-lg font-bold text-gray-900 hover:text-indigo-600 transition-colors flex items-center gap-1 cursor-pointer"
              >
                {position.title}
                <svg
                  xmlns="http://www.w3.org/2000/svg"
                  className="h-3.5 w-3.5 sm:h-4 sm:w-4 flex-shrink-0"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14"
                  />
                </svg>
              </a>
            ) : (
              <h3 className="text-base sm:text-lg font-bold text-gray-900">{position.title}</h3>
            )}
            <div className="mt-1">
              <span className="inline-block bg-blue-100 text-blue-700 px-2 sm:px-3 py-1 rounded-full text-xs font-semibold">
                {position.department}
              </span>
            </div>
          </div>

          <p className="text-sm text-gray-600 mb-3 sm:mb-4 leading-relaxed text-left">{position.description}</p>

          <div className="border-t border-gray-200 pt-3 sm:pt-4 space-y-1.5 sm:space-y-2">
            {(position.employees || []).map((emp) => (
              <div key={emp.id} className="flex items-center gap-2 sm:gap-3 p-1.5 sm:p-2 rounded-lg hover:bg-gray-50 transition-colors group">
                <UserAvatar 
                  email={emp.email}
                  userName={emp.name}
                  size="md" 
                  showName={true}
                  className="flex-1 min-w-0"
                  clickable={true}
                  onClick={() => handleViewUserDetail(emp)}
                />
                {emp.url && (
                  <a
                    href={emp.url}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="text-gray-400 hover:text-indigo-600 transition-colors ml-2"
                    title="View profile"
                  >
                    <svg
                      xmlns="http://www.w3.org/2000/svg"
                      className="h-4 w-4 flex-shrink-0"
                      fill="none"
                      viewBox="0 0 24 24"
                      stroke="currentColor"
                    >
                      <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        strokeWidth={2}
                        d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14"
                      />
                    </svg>
                  </a>
                )}
                {isEditMode && canEdit && (
                  <div className="flex gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                    <button
                      onClick={() => handleEditEmployee(emp, position.id)}
                      className="bg-blue-500 text-white p-1 rounded hover:bg-blue-600 transition-colors"
                      title="Edit employee"
                    >
                      <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z" />
                      </svg>
                    </button>
                    <button
                      onClick={() => handleDeleteEmployee(emp)}
                      className="bg-red-500 text-white p-1 rounded hover:bg-red-600 transition-colors"
                      title="Delete employee"
                    >
                      <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                      </svg>
                    </button>
                  </div>
                )}
              </div>
            ))}
            {isEditMode && canEdit && (
              <button
                onClick={() => handleCreateEmployee(position.id)}
                className="w-full mt-2 py-2 px-3 bg-green-50 text-green-700 border border-green-200 rounded-lg hover:bg-green-100 transition-colors flex items-center justify-center gap-2 text-sm font-semibold"
              >
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                </svg>
                Add Employee
              </button>
            )}
          </div>
        </div>

        {/* Children */}
        {children.length > 0 && (
          <div className="flex justify-center gap-4 sm:gap-8 md:gap-12 flex-wrap">{children.map((child) => renderPositionCard(child))}</div>
        )}
      </div>
    );
  };

  // Draw connection lines
  const renderConnections = () => {
    if (positionRects.length === 0) return null;

    const lines: JSX.Element[] = [];
    const filteredPositionIds = new Set(filteredPositions.map((p) => p.id));

    filteredPositions.forEach((position) => {
      if (!position.parentPositionId) return;

      // Only draw connection if parent is also in filtered positions
      if (!filteredPositionIds.has(position.parentPositionId)) return;

      const childRect = positionRects.find((r) => r.id === position.id);
      const parentRect = positionRects.find((r) => r.id === position.parentPositionId);

      if (!childRect || !parentRect) return;

      // Calculate connection points with spacing
      const verticalSpacing = 20; // Add 20px spacing from parent card
      const parentX = parentRect.x;
      const parentY = parentRect.y + parentRect.height + verticalSpacing;
      const childX = childRect.x;
      const childY = childRect.y - 10; // 10px before child card

      // Create path with vertical drop and horizontal connection
      const midY = parentY + (childY - parentY) / 2;

      const pathData = `M ${parentX} ${parentY} L ${parentX} ${midY} L ${childX} ${midY} L ${childX} ${childY}`;

      lines.push(
        <path
          key={`${position.parentPositionId}-${position.id}`}
          d={pathData}
          stroke="#94a3b8"
          strokeWidth="2"
          fill="none"
          strokeDasharray="0"
          className="transition-all"
        />
      );
    });

    return lines;
  };

  return (
    <div className="h-screen bg-gradient-to-br from-indigo-500 to-purple-600 flex flex-col">
      {/* Header */}
      <div className="bg-white border-b border-gray-200 p-3 md:p-4 shadow-sm">
        <div className="flex items-center justify-between mb-3 md:mb-4">
          <h1 className="text-lg md:text-2xl font-bold text-gray-900">{orgData.name} - Organizational Chart</h1>

          {/* Mobile menu button */}
          <button
            onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)}
            className="md:hidden p-2 rounded-lg hover:bg-gray-100 transition-colors"
            aria-label="Toggle menu"
          >
            <svg
              className="w-6 h-6 text-gray-700"
              fill="none"
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth="2"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              {isMobileMenuOpen ? (
                <path d="M6 18L18 6M6 6l12 12" />
              ) : (
                <path d="M4 6h16M4 12h16M4 18h16" />
              )}
            </svg>
          </button>
        </div>

        {/* Controls - hidden on mobile unless menu is open */}
        <div className={`flex flex-col gap-3 md:flex-row md:flex-wrap md:gap-4 md:items-center ${isMobileMenuOpen ? 'block' : 'hidden md:flex'}`}>
          {/* Filter controls */}
          <div className="flex flex-col sm:flex-row gap-3 md:contents">
            <div className="flex items-center gap-2 flex-1 sm:flex-none">
              <label className="font-semibold text-gray-700 text-sm whitespace-nowrap">Department:</label>
              <select
                value={filters.department}
                onChange={(e) => setFilters({ ...filters, department: e.target.value })}
                className="flex-1 sm:flex-none px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 text-sm"
              >
                <option value="all">All Departments</option>
                {departments.sort().map((dept) => (
                  <option key={dept} value={dept}>
                    {dept}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <button
            onClick={() => setFilters({ department: 'all' })}
            className="w-full sm:w-auto px-4 py-2 bg-indigo-600 text-white font-semibold rounded-lg hover:bg-indigo-700 transition-all hover:shadow-lg text-sm"
          >
            Reset Filters
          </button>

          {canEdit && (
            <>
              <button
                onClick={() => setIsEditMode(!isEditMode)}
                className={`w-full sm:w-auto px-4 py-2 font-semibold rounded-lg transition-all hover:shadow-lg text-sm ${
                  isEditMode
                    ? 'bg-green-600 text-white hover:bg-green-700'
                    : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
                }`}
              >
                {isEditMode ? 'Exit Edit Mode' : 'Edit Mode'}
              </button>

              {isEditMode && (
                <button
                  onClick={() => handleCreatePosition()}
                  className="w-full sm:w-auto px-4 py-2 bg-blue-600 text-white font-semibold rounded-lg hover:bg-blue-700 transition-all hover:shadow-lg text-sm"
                >
                  + Add Position
                </button>
              )}
            </>
          )}

          {/* Zoom controls */}
          <div className="flex items-center gap-2 justify-between sm:justify-start md:ml-4">
            <label className="font-semibold text-gray-700 text-sm">Zoom:</label>
            <div className="flex items-center gap-2">
              <button
                onClick={() => setZoom(Math.max(0.5, zoom - 0.1))}
                className="px-3 py-2 bg-gray-200 text-gray-700 font-bold rounded-lg hover:bg-gray-300 transition-all text-sm"
                disabled={zoom <= 0.5}
              >
                −
              </button>
              <span className="text-sm font-semibold text-gray-700 min-w-[50px] text-center">
                {Math.round(zoom * 100)}%
              </span>
              <button
                onClick={() => setZoom(Math.min(2, zoom + 0.1))}
                className="px-3 py-2 bg-gray-200 text-gray-700 font-bold rounded-lg hover:bg-gray-300 transition-all text-sm"
                disabled={zoom >= 2}
              >
                +
              </button>
              <button
                onClick={() => setZoom(1)}
                className="px-3 py-2 bg-gray-100 text-gray-600 text-xs rounded-lg hover:bg-gray-200 transition-all"
              >
                Reset
              </button>
            </div>
          </div>

          {/* Stats */}
          <div className="flex gap-3 justify-center sm:justify-start md:ml-auto">
            <div className="text-center px-3 py-1.5 bg-gray-50 border border-gray-200 rounded-lg flex-1 sm:flex-none">
              <div className="text-xs text-gray-500 uppercase tracking-wide">Positions</div>
              <div className="text-lg md:text-xl font-bold text-indigo-600">{filteredPositions.length}</div>
            </div>
            <div className="text-center px-3 py-1.5 bg-gray-50 border border-gray-200 rounded-lg flex-1 sm:flex-none">
              <div className="text-xs text-gray-500 uppercase tracking-wide">Employees</div>
              <div className="text-lg md:text-xl font-bold text-indigo-600">{totalEmployees}</div>
            </div>
          </div>
        </div>
      </div>

      {/* Mobile compact view */}
      <div className="md:hidden flex-1 overflow-auto bg-gradient-to-b from-gray-50 to-white">
        <div className="sticky top-0 bg-white border-b border-gray-200 p-2 z-10 flex gap-2">
          <button
            onClick={() => {
              const allIds = new Set(filteredPositions.map(p => p.id));
              setExpandedNodes(allIds);
            }}
            className="flex-1 px-3 py-1.5 bg-indigo-100 text-indigo-700 text-xs font-semibold rounded-lg hover:bg-indigo-200 transition-colors"
          >
            Expand All
          </button>
          <button
            onClick={() => setExpandedNodes(new Set())}
            className="flex-1 px-3 py-1.5 bg-gray-100 text-gray-700 text-xs font-semibold rounded-lg hover:bg-gray-200 transition-colors"
          >
            Collapse All
          </button>
        </div>
        <div className="p-3 space-y-2">
          {buildTree(filteredPositions).map((root) => renderCompactPosition(root))}
        </div>
      </div>

      {/* Desktop graphical view */}
      <div className="hidden md:block flex-1 overflow-auto bg-gradient-to-b from-gray-50 to-white">
        <div
          className="p-10 min-w-max relative origin-top-left transition-transform duration-200"
          ref={containerRef}
          style={{ transform: `scale(${zoom})` }}
        >
          {/* SVG for connection lines */}
          <svg className="absolute top-0 left-0 w-full h-full pointer-events-none" style={{ zIndex: 0 }}>
            {renderConnections()}
          </svg>

          {/* Position cards - Hierarchical Tree Layout */}
          <div className="relative" style={{ zIndex: 1 }}>
            <div className="flex justify-center gap-12">
              {buildTree(filteredPositions).map((root) => renderPositionCard(root))}
            </div>
          </div>
        </div>
      </div>

      {/* Modals */}
      <PositionModal
        isOpen={positionModalOpen}
        onClose={() => setPositionModalOpen(false)}
        onSave={handleSavePosition}
        position={editingPosition}
        positions={orgData?.positions || []}
        mode={positionModalMode}
        parentPositionId={currentPositionId}
      />

      <EmployeeModal
        isOpen={employeeModalOpen}
        onClose={() => setEmployeeModalOpen(false)}
        onSave={handleSaveEmployee}
        employee={editingEmployee}
        positions={orgData?.positions || []}
        currentPositionId={currentPositionId}
        mode={employeeModalMode}
      />

      <ConfirmDialog
        isOpen={confirmDialogOpen}
        title={confirmDialogConfig.title}
        message={confirmDialogConfig.message}
        onConfirm={confirmDialogConfig.onConfirm}
        onCancel={() => setConfirmDialogOpen(false)}
        variant="danger"
      />

      {selectedEmployee && (
        <UserDetailModal
          isOpen={userDetailModalOpen}
          onClose={() => {
            setUserDetailModalOpen(false);
            setSelectedEmployee(null);
          }}
          employee={selectedEmployee}
        />
      )}
    </div>
  );
};

export default OrgChart;