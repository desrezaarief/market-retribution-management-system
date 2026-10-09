using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RetribusiPasar.Web.Infrastructure.Repositories;
using RetribusiPasar.Web.Models.Entities;
namespace RetribusiPasar.Web.Controllers;
[Authorize(Roles="Administrator,Bendahara")]
public class AuditController:Controller{
 private readonly IRepository<AuditLog> _repo;public AuditController(IRepository<AuditLog> repo)=>_repo=repo;
 public async Task<IActionResult>Index()=>View((await _repo.GetAllAsync()).OrderByDescending(x=>x.Timestamp).ToList());
}
