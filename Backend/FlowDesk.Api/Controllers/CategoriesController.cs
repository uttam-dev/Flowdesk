using AutoMapper;
using FlowDesk.Application.Features.Categories.Commands;
using FlowDesk.Application.Features.Categories.DTOs;
using FlowDesk.Application.Features.Categories.Queries;
using FlowDesk.Domain.DTOs;
using FlowDesk.Domain.Enums;
using FlowDesk.Domain.Utils;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FlowDesk.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController(IMediator _mediator, IMapper _mapper) : ControllerBase
    {
        // GET: api/categories
        //[Authorize(policy: "RequireAdminRole")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] FilterCategoryDataQueryDto query)
        {

            var result = await _mediator.Send(new GetAllCategoriesQuery(query));


            var adminData = new PagedResult<CategoryAdminResponseDto>
            {
                Items = _mapper.Map<List<CategoryAdminResponseDto>>(result.Items),
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };

            return Ok(new ApiResponseDto
            {
                Message = "Categories fetched successfully",
                Data = adminData
            });

        }

        // GET: api/categories/active
        //[Authorize]
        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
        {
            var result = await _mediator.Send(new GetActiveCategoriesQuery());
            return Ok(new ApiResponseDto() { Message = "Active categories fetched successfully", Data = result});
        }

        // GET: api/categories/{id}
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _mediator.Send(new GetCategoryByIdQuery(id));

            if (User.IsInRole(RoleEnum.Admin.ToString()))
            {
                var adminData = _mapper.Map<CategoryAdminResponseDto>(result);
                return Ok(new ApiResponseDto() { Message = "Category fatched successfully", Data = adminData });
            }

            var basicData = _mapper.Map<CategoryBasicResponseDto>(result);
            return Ok(new ApiResponseDto() { Message = "Category fatched successfully", Data = basicData });
        }

        // POST: api/categories
        [Authorize(policy: "RequireAdminRole")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryDto dto)
        {
            var result = await _mediator.Send(new CreateCategoryCommand(dto));
            return Ok(new ApiResponseDto() { Message = "Category created successfully", Data = result });
        }

        // PUT: api/categories/{id}
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateCategoryDto dto)
        {
            var result = await _mediator.Send(new UpdateCategoryCommand(id, dto));
            return Ok(new ApiResponseDto() { Message = "Category updated successfully", Data = result });
        }

        // PATCH: api/categories/{id}/active
        [Authorize(policy: "RequireAdminRole")]
        [HttpPatch("{id}/active")]
        public async Task<IActionResult> Activate(int id)
        {
            var result = await _mediator.Send(new ActivateCategoryCommand(id));
            return Ok(new ApiResponseDto() { Message = "Category activate successfully" });
        }

        // PATCH: api/categories/{id}/deactive
        [Authorize(policy: "RequireAdminRole")]
        [HttpPatch("{id}/deactive")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var result = await _mediator.Send(new DeactivateCategoryCommand(id));
            return Ok(new ApiResponseDto() { Message = "Category deactive successfully" });
        }

    }
}
