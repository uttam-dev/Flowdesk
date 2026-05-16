using AutoMapper;
using FlowDesk.Application.Features.Categories.DTOs;
using FlowDesk.Application.Features.Users.DTOs;
using FlowDesk.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Application.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //User
            CreateMap<CreateUserDto, User>();
            CreateMap<User, UserResponseDto>();
            CreateMap<UpdateUserDetailsDto, User>()
                .ForAllMembers(opts =>
                    opts.Condition((src, dest, srcMember) => srcMember != null));

            //Category
            CreateMap<Category, CategoryAdminResponseDto>();
            CreateMap<Category, CategoryBasicResponseDto>();
            CreateMap<CategoryResponseDto,CategoryAdminResponseDto>();
            CreateMap<CategoryResponseDto,CategoryBasicResponseDto>();
            CreateMap<UpdateCategoryDto, Category>();
            CreateMap<Category, CategoryResponseDto>();

            
        }
    }
}
