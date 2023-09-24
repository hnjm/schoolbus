﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Features.Parents.DTOs;
using CleanArchitecture.Blazor.Application.Features.Parents.Caching;
namespace CleanArchitecture.Blazor.Application.Features.Parents.Commands.AddEdit;

public class AddEditParentCommand: ICacheInvalidatorRequest<Result<int>>
{
    [Description("Id")]
    public int Id { get; set; }
    [Description("Last Name")]
    public string? LastName {get;set;} 
    [Description("First Name")]
    public string? FirstName {get;set;}
    [Description("Display Name")]
    public string? DisplayName => $"{LastName} {FirstName}";
    [Description("Profile Picture")]
    public string? ProfilePicture {get;set;} 
    [Description("Phone")]
    public string? Phone {get;set;} 
    [Description("Description")]
    public string? Description {get;set;} 
    [Description("Status")]
    public string? Status {get;set;} 
    [Description("Tenant Id")]
    public string? TenantId {get;set;}

    public int[] Children { get; set; } = Array.Empty<int>();


      public string CacheKey => ParentCacheKey.GetAllCacheKey;
      public CancellationTokenSource? SharedExpiryTokenSource => ParentCacheKey.SharedExpiryTokenSource();

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<ParentDto,AddEditParentCommand>(MemberList.None);
            CreateMap<AddEditParentCommand, Parent>(MemberList.None);
        }
    }
}

    public class AddEditParentCommandHandler : IRequestHandler<AddEditParentCommand, Result<int>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<AddEditParentCommandHandler> _localizer;
        public AddEditParentCommandHandler(
            IApplicationDbContext context,
            IStringLocalizer<AddEditParentCommandHandler> localizer,
            IMapper mapper
            )
        {
            _context = context;
            _localizer = localizer;
            _mapper = mapper;
        }
        public async Task<Result<int>> Handle(AddEditParentCommand request, CancellationToken cancellationToken)
        {
            if (request.Id > 0)
            {
                var item = await _context.Parents.FindAsync(new object[] { request.Id }, cancellationToken) ?? throw new NotFoundException($"Parent with id: [{request.Id}] not found.");
                item = _mapper.Map(request, item);
				// raise a update domain event
				item.AddDomainEvent(new ParentUpdatedEvent(item));
                await _context.SaveChangesAsync(cancellationToken);
                return await Result<int>.SuccessAsync(item.Id);
            }
            else
            {
                var item = _mapper.Map<Parent>(request);
                // raise a create domain event
				item.AddDomainEvent(new ParentCreatedEvent(item));
                _context.Parents.Add(item);
                await _context.SaveChangesAsync(cancellationToken);
                return await Result<int>.SuccessAsync(item.Id);
            }
           
        }
    }

