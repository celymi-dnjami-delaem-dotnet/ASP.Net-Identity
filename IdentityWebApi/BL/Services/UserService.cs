using System;
using System.Threading.Tasks;
using AutoMapper;
using IdentityWebApi.BL.Constants;
using IdentityWebApi.BL.Enums;
using IdentityWebApi.BL.Interfaces;
using IdentityWebApi.BL.ResultWrappers;
using IdentityWebApi.DAL.Entities;
using IdentityWebApi.DAL.Interfaces;
using IdentityWebApi.PL.Models.DTO;

namespace IdentityWebApi.BL.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        
        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResult<UserDto>> GetUserAsync(Guid id)
        {
            var user = await _userRepository.SearchForSingleItemAsync(x => x.Id == id, inc => inc.UserRoles);
            if (user is null)
            {
                return new ServiceResult<UserDto>(ServiceResultType.NotFound, ExceptionMessageConstants.MissingUser);
            }

            return new ServiceResult<UserDto>(ServiceResultType.Success,_mapper.Map<UserDto>(user));
        }

        public async Task<ServiceResult<UserDto>> CreateUserAsync(UserDto user)
        {
            var userEntity = _mapper.Map<AppUser>(user);
            
            var createdUserResult = await _userRepository.CreateUserAsync(userEntity, user.Password, user.UserRole, true);

            return new ServiceResult<UserDto>(
                createdUserResult.ServiceResultType, 
                createdUserResult.Message,
                createdUserResult.Data.appUser is not null ? _mapper.Map<UserDto>(createdUserResult.Data.appUser) : default
            );
        }

        public async Task<ServiceResult<UserDto>> UpdateUserAsync(UserDto user)
        {
            var userEntity = _mapper.Map<AppUser>(user);

            var updatedUserResult = await _userRepository.UpdateUserAsync(userEntity);

            return new ServiceResult<UserDto>(
                updatedUserResult.ServiceResultType,
                updatedUserResult.Message,
                updatedUserResult.Data is not null ? _mapper.Map<UserDto>(updatedUserResult.Data) : default
            );
        }

        public async Task<ServiceResult> RemoveUserAsync(Guid id)
        {
            return await _userRepository.RemoveUserAsync(id);
        }
    }
}