import React from 'react';
import { useUserProfile, useUserPhoto } from '../../services/hooks';

interface UserAvatarProps {
  email: string;
  size?: 'sm' | 'md' | 'lg';
  showName?: boolean;
  showDetails?: boolean;
  className?: string;
}

/**
 * Component that displays user avatar with photo from Microsoft Graph
 */
export const UserAvatar: React.FC<UserAvatarProps> = ({
  email,
  size = 'md',
  showName = false,
  showDetails = false,
  className = '',
}) => {
  const { data: userProfile, isLoading: profileLoading, error: profileError } = useUserProfile(email);
  const { data: userPhoto, isLoading: photoLoading } = useUserPhoto(email);

  // Size configurations
  const sizeClasses = {
    sm: 'w-8 h-8 text-xs',
    md: 'w-12 h-12 text-sm',
    lg: 'w-16 h-16 text-base',
  };

  const textSizeClasses = {
    sm: 'text-xs',
    md: 'text-sm',
    lg: 'text-base',
  };

  // Generate initials from name or email
  const getInitials = (name?: string, email?: string) => {
    if (name) {
      return name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2);
    }
    if (email) {
      return email.slice(0, 2).toUpperCase();
    }
    return '?';
  };

  const initials = getInitials(userProfile?.displayName, email);

  return (
    <div className={`flex items-center gap-3 ${className}`}>
      {/* Avatar Image/Initials */}
      <div className={`${sizeClasses[size]} relative rounded-full overflow-hidden bg-blue-500 text-white flex items-center justify-center font-medium shadow-sm`}>
        {userPhoto?.dataUrl && !photoLoading ? (
          <img
            src={userPhoto.dataUrl}
            alt={userProfile?.displayName || email}
            className="w-full h-full object-cover"
            onError={(e) => {
              // Hide image on error and show initials
              (e.target as HTMLImageElement).style.display = 'none';
            }}
          />
        ) : (
          <span className={textSizeClasses[size]}>{initials}</span>
        )}
        
        {/* Loading indicator */}
        {(profileLoading || photoLoading) && (
          <div className="absolute inset-0 bg-gray-200 animate-pulse rounded-full" />
        )}
      </div>

      {/* User Information */}
      {(showName || showDetails) && (
        <div className="flex flex-col min-w-0">
          {showName && (
            <div className={`font-medium text-gray-900 truncate ${textSizeClasses[size]}`}>
              {profileLoading ? (
                <div className="bg-gray-200 animate-pulse h-4 w-24 rounded" />
              ) : (
                userProfile?.displayName || email.split('@')[0]
              )}
            </div>
          )}
          
          {showDetails && userProfile && (
            <div className="space-y-1">
              {userProfile.jobTitle && (
                <div className="text-xs text-gray-600 truncate">
                  {userProfile.jobTitle}
                </div>
              )}
              {userProfile.department && (
                <div className="text-xs text-gray-500 truncate">
                  {userProfile.department}
                </div>
              )}
              {(userProfile.mobilePhone || userProfile.businessPhone) && (
                <div className="text-xs text-gray-500 truncate">
                  {userProfile.mobilePhone || userProfile.businessPhone}
                </div>
              )}
            </div>
          )}
          
          {profileError && (
            <div className="text-xs text-gray-400 italic">
              Profile not found
            </div>
          )}
        </div>
      )}
    </div>
  );
};

export default UserAvatar;