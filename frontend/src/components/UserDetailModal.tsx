import React from 'react';
import { useUserProfile, useUserPhoto } from '../services/hooks';
import { Employee } from '../types/orgchart';

interface UserDetailModalProps {
  isOpen: boolean;
  onClose: () => void;
  employee: Employee;
}

/**
 * Modal component showing detailed user information from Microsoft Graph
 */
export const UserDetailModal: React.FC<UserDetailModalProps> = ({
  isOpen,
  onClose,
  employee,
}) => {
  const { data: userProfile, isLoading: profileLoading, error: profileError } = useUserProfile(employee.email);
  const { data: userPhoto, isLoading: photoLoading } = useUserPhoto(employee.email);

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-lg shadow-xl max-w-md w-full max-h-[90vh] overflow-y-auto">
        {/* Header */}
        <div className="flex justify-between items-center p-6 border-b border-gray-200">
          <h2 className="text-xl font-semibold text-gray-900">
            User Details
          </h2>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600 transition-colors"
          >
            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        {/* Content */}
        <div className="p-6">
          {/* Profile Photo */}
          <div className="flex justify-center mb-6">
            <div className="w-32 h-32 relative rounded-full overflow-hidden bg-blue-500 text-white flex items-center justify-center shadow-lg">
              {userPhoto?.dataUrl && !photoLoading ? (
                <img
                  src={userPhoto.dataUrl}
                  alt={userProfile?.displayName || employee.name}
                  className="w-full h-full object-cover"
                  onError={(e) => {
                    (e.target as HTMLImageElement).style.display = 'none';
                  }}
                />
              ) : (
                <span className="text-3xl font-bold">
                  {(userProfile?.displayName || employee.name)
                    .split(' ')
                    .map(n => n[0])
                    .join('')
                    .toUpperCase()
                    .slice(0, 2)}
                </span>
              )}
              
              {(profileLoading || photoLoading) && (
                <div className="absolute inset-0 bg-gray-200 animate-pulse rounded-full" />
              )}
            </div>
          </div>

          {/* User Information */}
          <div className="space-y-4">
            {/* Name */}
            <div className="text-center">
              <h3 className="text-xl font-semibold text-gray-900">
                {profileLoading ? (
                  <div className="bg-gray-200 animate-pulse h-6 w-32 rounded mx-auto" />
                ) : (
                  userProfile?.displayName || employee.name
                )}
              </h3>
              <p className="text-sm text-gray-600 mt-1">{employee.email}</p>
            </div>

            {/* Microsoft Graph Information */}
            {userProfile && (
              <div className="border-t border-gray-200 pt-4">
                <div className="space-y-2">
                  {userProfile.jobTitle && (
                    <div className="flex">
                      <span className="text-sm text-gray-600 w-24 flex-shrink-0">Title:</span>
                      <span className="text-sm text-gray-900">{userProfile.jobTitle}</span>
                    </div>
                  )}
                  
                  {userProfile.department && (
                    <div className="flex">
                      <span className="text-sm text-gray-600 w-24 flex-shrink-0">Department:</span>
                      <span className="text-sm text-gray-900">{userProfile.department}</span>
                    </div>
                  )}

                  {userProfile.companyName && (
                    <div className="flex">
                      <span className="text-sm text-gray-600 w-24 flex-shrink-0">Company:</span>
                      <span className="text-sm text-gray-900">{userProfile.companyName}</span>
                    </div>
                  )}

                  {userProfile.employeeId && (
                    <div className="flex">
                      <span className="text-sm text-gray-600 w-24 flex-shrink-0">Employee ID:</span>
                      <span className="text-sm text-gray-900">{userProfile.employeeId}</span>
                    </div>
                  )}

                  {userProfile.hireDate && userProfile.hireDate !== '0001-01-01' && (
                    <div className="flex">
                      <span className="text-sm text-gray-600 w-24 flex-shrink-0">Hire Date:</span>
                      <span className="text-sm text-gray-900">{userProfile.hireDate}</span>
                    </div>
                  )}

                  {userProfile.birthday && userProfile.birthday !== '0001-01-01' && (
                    <div className="flex">
                      <span className="text-sm text-gray-600 w-24 flex-shrink-0">Birthday:</span>
                      <span className="text-sm text-gray-900">{userProfile.birthday}</span>
                    </div>
                  )}
                  
                  {userProfile.officeLocation && (
                    <div className="flex">
                      <span className="text-sm text-gray-600 w-24 flex-shrink-0">Office:</span>
                      <span className="text-sm text-gray-900">{userProfile.officeLocation}</span>
                    </div>
                  )}

                  {(() => {
                    const locationParts = [userProfile.city, userProfile.country]
                      .filter(Boolean)
                      .filter((part): part is string => part != null && part.trim() !== '' && part !== '000' && !part.match(/^0+$/));
                    return locationParts.length > 0 && (
                      <div className="flex">
                        <span className="text-sm text-gray-600 w-24 flex-shrink-0">Location:</span>
                        <span className="text-sm text-gray-900">
                          {locationParts.join(', ')}
                        </span>
                      </div>
                    );
                  })()}

                  {userProfile.mobilePhone && (
                    <div className="flex">
                      <span className="text-sm text-gray-600 w-24 flex-shrink-0">Mobile:</span>
                      <span className="text-sm text-gray-900">
                        <a href={`tel:${userProfile.mobilePhone}`} className="text-blue-600 hover:text-blue-800">
                          {userProfile.mobilePhone}
                        </a>
                      </span>
                    </div>
                  )}
                  
                  {userProfile.businessPhone && (
                    <div className="flex">
                      <span className="text-sm text-gray-600 w-24 flex-shrink-0">Business:</span>
                      <span className="text-sm text-gray-900">
                        <a href={`tel:${userProfile.businessPhone}`} className="text-blue-600 hover:text-blue-800">
                          {userProfile.businessPhone}
                        </a>
                      </span>
                    </div>
                  )}

                  {userProfile.homePhone && (
                    <div className="flex">
                      <span className="text-sm text-gray-600 w-24 flex-shrink-0">Home:</span>
                      <span className="text-sm text-gray-900">
                        <a href={`tel:${userProfile.homePhone}`} className="text-blue-600 hover:text-blue-800">
                          {userProfile.homePhone}
                        </a>
                      </span>
                    </div>
                  )}

                  {userProfile.preferredLanguage && (
                    <div className="flex">
                      <span className="text-sm text-gray-600 w-24 flex-shrink-0">Language:</span>
                      <span className="text-sm text-gray-900">{userProfile.preferredLanguage}</span>
                    </div>
                  )}

                  {userProfile.manager && (
                    <>
                      <div className="flex">
                        <span className="text-sm text-gray-600 w-24 flex-shrink-0">Manager:</span>
                        <span className="text-sm text-gray-900">{userProfile.manager.displayName}</span>
                      </div>
                      {userProfile.manager.email && (
                        <div className="flex">
                          <span className="text-sm text-gray-600 w-24 flex-shrink-0">Manager Email:</span>
                          <span className="text-sm text-gray-900">
                            <a href={`mailto:${userProfile.manager.email}`} className="text-blue-600 hover:text-blue-800">
                              {userProfile.manager.email}
                            </a>
                          </span>
                        </div>
                      )}
                      {userProfile.manager.jobTitle && (
                        <div className="flex">
                          <span className="text-sm text-gray-600 w-24 flex-shrink-0">Manager Title:</span>
                          <span className="text-sm text-gray-900">{userProfile.manager.jobTitle}</span>
                        </div>
                      )}
                    </>
                  )}

                  {userProfile.aboutMe && (
                    <div>
                      <span className="text-sm text-gray-600 block mb-1">About:</span>
                      <p className="text-sm text-gray-900">{userProfile.aboutMe}</p>
                    </div>
                  )}

                  {userProfile.interests?.length && (
                    <div>
                      <span className="text-sm text-gray-600 block mb-1">Interests:</span>
                      <div className="flex flex-wrap gap-1">
                        {userProfile.interests.map((interest, index) => (
                          <span key={index} className="text-xs bg-blue-100 text-blue-800 px-2 py-1 rounded">
                            {interest}
                          </span>
                        ))}
                      </div>
                    </div>
                  )}

                  {userProfile.skills?.length && (
                    <div>
                      <span className="text-sm text-gray-600 block mb-1">Skills:</span>
                      <div className="flex flex-wrap gap-1">
                        {userProfile.skills.map((skill, index) => (
                          <span key={index} className="text-xs bg-green-100 text-green-800 px-2 py-1 rounded">
                            {skill}
                          </span>
                        ))}
                      </div>
                    </div>
                  )}

                  {userProfile.responsibilities?.length && (
                    <div>
                      <span className="text-sm text-gray-600 block mb-1">Responsibilities:</span>
                      <div className="flex flex-wrap gap-1">
                        {userProfile.responsibilities.map((resp, index) => (
                          <span key={index} className="text-xs bg-purple-100 text-purple-800 px-2 py-1 rounded">
                            {resp}
                          </span>
                        ))}
                      </div>
                    </div>
                  )}
                </div>
              </div>
            )}

            {/* Organization Chart Information */}
            <div className="border-t border-gray-200 pt-4">
              <div className="space-y-2">
                <div className="flex">
                  <span className="text-sm text-gray-600 w-24 flex-shrink-0">Name:</span>
                  <span className="text-sm text-gray-900">{employee.name}</span>
                </div>
                
                <div className="flex">
                  <span className="text-sm text-gray-600 w-24 flex-shrink-0">Email:</span>
                  <span className="text-sm text-gray-900">
                    <a href={`mailto:${employee.email}`} className="text-blue-600 hover:text-blue-800">
                      {employee.email}
                    </a>
                  </span>
                </div>
                
                {employee.startDate && (
                  <div className="flex">
                    <span className="text-sm text-gray-600 w-24 flex-shrink-0">Start Date:</span>
                    <span className="text-sm text-gray-900">{employee.startDate}</span>
                  </div>
                )}
                
                {employee.url && (
                  <div className="flex">
                    <span className="text-sm text-gray-600 w-24 flex-shrink-0">Profile:</span>
                    <span className="text-sm text-gray-900">
                      <a
                        href={employee.url}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="text-blue-600 hover:text-blue-800 flex items-center gap-1"
                      >
                        View Profile
                        <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14" />
                        </svg>
                      </a>
                    </span>
                  </div>
                )}
              </div>
            </div>

            {/* Error state */}
            {profileError && (
              <div className="border-t border-gray-200 pt-4">
                <div className="bg-yellow-50 border border-yellow-200 rounded-md p-3">
                  <div className="flex">
                    <svg className="w-5 h-5 text-yellow-400 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
                      <path fillRule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
                    </svg>
                    <div className="ml-3">
                      <p className="text-sm text-yellow-800">
                        Microsoft Graph profile information could not be loaded.
                      </p>
                    </div>
                  </div>
                </div>
              </div>
            )}
          </div>
        </div>

        {/* Footer */}
        <div className="border-t border-gray-200 px-6 py-4">
          <button
            onClick={onClose}
            className="w-full bg-gray-100 text-gray-700 py-2 px-4 rounded-md hover:bg-gray-200 transition-colors"
          >
            Close
          </button>
        </div>
      </div>
    </div>
  );
};

export default UserDetailModal;