import React, { useState, useEffect } from 'react';
import { Position, CreatePositionRequest, UpdatePositionRequest } from '../types/orgchart';

interface PositionModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSave: (data: CreatePositionRequest | UpdatePositionRequest) => void;
  position?: Position | null;
  positions: Position[];
  mode: 'create' | 'edit';
  parentPositionId?: string;
}

const PositionModal: React.FC<PositionModalProps> = ({ isOpen, onClose, onSave, position, positions, mode, parentPositionId }) => {
  const [formData, setFormData] = useState<CreatePositionRequest | UpdatePositionRequest>({
    title: '',
    description: '',
    department: '',
    parentPositionId: undefined,
    url: undefined,
  });

  useEffect(() => {
    if (position && mode === 'edit') {
      setFormData({
        title: position.title,
        description: position.description,
        department: position.department,
        parentPositionId: position.parentPositionId,
        url: position.url,
      });
    } else if (mode === 'create') {
      setFormData({
        title: '',
        description: '',
        department: '',
        parentPositionId: parentPositionId || undefined,
        url: undefined,
      });
    }
  }, [position, mode, parentPositionId]);

  if (!isOpen) return null;

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSave(formData);
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value || undefined,
    }));
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-lg shadow-xl max-w-2xl w-full max-h-[90vh] overflow-y-auto">
        <div className="p-6">
          <h2 className="text-2xl font-bold text-gray-900 mb-6">
            {mode === 'create' ? 'Create New Position' : 'Edit Position'}
          </h2>

          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="block text-sm font-semibold text-gray-700 mb-2">Title *</label>
              <input
                type="text"
                name="title"
                value={formData.title}
                onChange={handleChange}
                required
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500"
              />
            </div>

            <div>
              <label className="block text-sm font-semibold text-gray-700 mb-2">Description *</label>
              <textarea
                name="description"
                value={formData.description}
                onChange={handleChange}
                required
                rows={3}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500"
              />
            </div>

            <div>
              <label className="block text-sm font-semibold text-gray-700 mb-2">Department *</label>
              <input
                type="text"
                name="department"
                value={formData.department}
                onChange={handleChange}
                required
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500"
              />
            </div>

            <div>
              <label className="block text-sm font-semibold text-gray-700 mb-2">Parent Position</label>
              <select
                name="parentPositionId"
                value={formData.parentPositionId || ''}
                onChange={handleChange}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500"
              >
                <option value="">None (Root Position)</option>
                {positions
                  .filter((p) => !position || p.id !== position.id)
                  .map((p) => (
                    <option key={p.id} value={p.id}>
                      {p.title} ({p.department})
                    </option>
                  ))}
              </select>
            </div>

            <div>
              <label className="block text-sm font-semibold text-gray-700 mb-2">URL</label>
              <input
                type="url"
                name="url"
                value={formData.url || ''}
                onChange={handleChange}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500"
              />
            </div>

            <div className="flex gap-3 pt-4">
              <button
                type="submit"
                className="flex-1 px-4 py-2 bg-indigo-600 text-white font-semibold rounded-lg hover:bg-indigo-700 transition-colors"
              >
                {mode === 'create' ? 'Create Position' : 'Save Changes'}
              </button>
              <button
                type="button"
                onClick={onClose}
                className="flex-1 px-4 py-2 bg-gray-200 text-gray-700 font-semibold rounded-lg hover:bg-gray-300 transition-colors"
              >
                Cancel
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
};

export default PositionModal;
