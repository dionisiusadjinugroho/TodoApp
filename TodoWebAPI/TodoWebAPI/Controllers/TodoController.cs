using BCrypt.Net;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Net;
using TodoWebAPI.Data;
using TodoWebAPI.Helper;
using TodoWebAPI.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TodoWebAPI.Controllers
{
    [Route("api/Todo")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private ToDoContext _toDoContext;
        private Utility _utility;
        private IMemoryCache _memoryCache;

        public TodoController(ToDoContext toDoContext, Utility utility, IMemoryCache memoryCache) 
        {
            _toDoContext = toDoContext;
            _utility = utility;
            _memoryCache = memoryCache;
        }

        [HttpGet("User")]
        public async Task<ActionResult> UserInfo([FromQuery] string UserID)
        {
            try
            {
                var user = await _toDoContext.Users.Where(x => x.UserId == Convert.ToInt32(UserID)).FirstOrDefaultAsync();

                _utility.WriteToFile(this.Request.Method, this.Request.GetDisplayUrl(), UserID, JsonConvert.SerializeObject(user), HttpStatusCode.OK.ToString());
                return Ok(user);
            }
            catch (Exception ex)
            {
                _utility.WriteToFile(this.Request.Method, this.Request.GetDisplayUrl(), UserID, JsonConvert.SerializeObject(ex.Message), HttpStatusCode.InternalServerError.ToString());
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult> GetAllData([FromQuery] string UserID, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {

                var cacheKey = $"Activities_{UserID}_Page_{page}_Size_{pageSize}";
                if (_memoryCache.TryGetValue(cacheKey, out var cachedData))
                {
                    return Ok(cachedData);
                }

                var skip = (page - 1) * pageSize;

                var getalldata = await (from a in _toDoContext.Users
                                        join b in _toDoContext.Activities on a.UserId equals b.UserId
                                        where a.UserId == Convert.ToInt32(UserID)
                                        select b)
                                         .Skip(skip)
                                         .Take(pageSize)
                                         .ToListAsync();

                var totalRecords = await _toDoContext.Activities.CountAsync(a => a.UserId == Convert.ToInt32(UserID));

                var result = new
                {
                    Data = getalldata,
                    TotalRecords = totalRecords
                };

                _memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

                _utility.WriteToFile(this.Request.Method, this.Request.GetDisplayUrl(), "", JsonConvert.SerializeObject(result), HttpStatusCode.OK.ToString());
                return Ok(result);
            }
            catch (Exception ex)
            {
                _utility.WriteToFile(this.Request.Method, this.Request.GetDisplayUrl(), "", JsonConvert.SerializeObject(ex.Message), HttpStatusCode.InternalServerError.ToString());
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("Login")]
        public async Task<ActionResult> Login([FromBody] UserLogin loginmodel)
        {
            try
            {
                var validUser = await _toDoContext.Users.Where(x => x.Username == loginmodel.Username).FirstOrDefaultAsync();
                if (validUser != null)
                {
                    if (BCrypt.Net.BCrypt.Verify(loginmodel.Password, validUser.PasswordHash))
                    {
                        _utility.WriteToFile(this.Request.Method, this.Request.GetDisplayUrl(), JsonConvert.SerializeObject(loginmodel), JsonConvert.SerializeObject(validUser), HttpStatusCode.OK.ToString());
                        return Ok(validUser);
                    }
                }
                string message = "Username/Password not valid";
                _utility.WriteToFile(this.Request.Method, this.Request.GetDisplayUrl(), JsonConvert.SerializeObject(loginmodel), message, HttpStatusCode.BadRequest.ToString());
                return BadRequest(message);
            }
            catch (Exception ex)
            {
                _utility.WriteToFile(this.Request.Method, this.Request.GetDisplayUrl(), JsonConvert.SerializeObject(loginmodel), JsonConvert.SerializeObject(ex.Message), HttpStatusCode.InternalServerError.ToString());
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("Register")]
        public async Task<ActionResult> Register([FromBody] UserRegister registermodel)
        {
            try
            {
                var PasswordHash = BCrypt.Net.BCrypt.HashPassword(registermodel.Password);
                var existingUser = await _toDoContext.Users.Where(x => x.Username == registermodel.Username || x.Name == registermodel.Name).FirstOrDefaultAsync();
                if (existingUser == null)
                {
                    User user = new User
                    {
                        Name = registermodel.Name,
                        PasswordHash = PasswordHash,
                        Username = registermodel.Username,
                        CreatedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        UpdatedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    };

                    _toDoContext.Users.Add(user);
                    await _toDoContext.SaveChangesAsync();

                    _utility.WriteToFile(this.Request.Method, this.Request.GetDisplayUrl(), JsonConvert.SerializeObject(registermodel), JsonConvert.SerializeObject(existingUser), HttpStatusCode.OK.ToString());
                    return Ok(existingUser);
                }
                else
                {
                    string message = "Username/Name already exist";
                    _utility.WriteToFile(this.Request.Method, this.Request.GetDisplayUrl(), JsonConvert.SerializeObject(registermodel), message, HttpStatusCode.BadRequest.ToString());
                    return BadRequest(message);
                }
            }
            catch (Exception ex)
            {
                _utility.WriteToFile(this.Request.Method, this.Request.GetDisplayUrl(), JsonConvert.SerializeObject(registermodel), JsonConvert.SerializeObject(ex.Message), HttpStatusCode.InternalServerError.ToString());
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPost("UpdateActivities")]
        public async Task<ActionResult> UpdateActivities([FromBody] Activity activity)
        {
            try
            {
                var isvalidActivities = await _toDoContext.Activities.Where(x => x.ActivitiesNo == activity.ActivitiesNo && x.UserId != activity.UserId).FirstOrDefaultAsync();

                if (isvalidActivities != null)
                {
                    string message = "Cant update as Activities No already exist on another User.";
                    _utility.WriteToFile(this.Request.Method, this.Request.GetDisplayUrl(), JsonConvert.SerializeObject(activity), JsonConvert.SerializeObject(message), HttpStatusCode.BadRequest.ToString());
                    return BadRequest(message);
                }

                var validActivities = await _toDoContext.Activities.Where(x => x.ActivitiesNo == activity.ActivitiesNo && x.UserId == activity.UserId).FirstOrDefaultAsync();
                if (validActivities != null)
                {
                    validActivities.Subject = activity.Subject;
                    validActivities.Description = activity.Description;
                    validActivities.Status = activity.Status;
                    validActivities.UpdatedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    await _toDoContext.SaveChangesAsync();

                    _utility.WriteToFile(this.Request.Method, this.Request.GetDisplayUrl(), JsonConvert.SerializeObject(activity), JsonConvert.SerializeObject(validActivities), HttpStatusCode.OK.ToString());
                    return Ok(validActivities);
                }
                else
                {
                    string message = "Activities No not exist";
                    _utility.WriteToFile(this.Request.Method, this.Request.GetDisplayUrl(), JsonConvert.SerializeObject(activity), JsonConvert.SerializeObject(message), HttpStatusCode.BadRequest.ToString());
                    return BadRequest(message);
                }
            }
            catch (Exception ex)
            {
                _utility.WriteToFile(this.Request.Method, this.Request.GetDisplayUrl(), JsonConvert.SerializeObject(activity), JsonConvert.SerializeObject(ex.Message), HttpStatusCode.InternalServerError.ToString());
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPost("InsertActivities")]
        public async Task<ActionResult> InsertActivities([FromBody] Activity activity)
        {
            try
            {
                string uniqueActivityNo = string.Empty;
                using (var transaction = await _toDoContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var userValid = await _toDoContext.Users.Where(x => x.UserId == activity.UserId).FirstOrDefaultAsync();
                        if (userValid == null)
                        {
                            string message = "User not valid";
                            _utility.WriteToFile(this.Request.Method, this.Request.GetDisplayUrl(), JsonConvert.SerializeObject(activity), JsonConvert.SerializeObject(message), HttpStatusCode.BadRequest.ToString());
                            return BadRequest(message);
                        }

                        var lastActivity = await _toDoContext.Activities.OrderByDescending(x => x.ActivitiesNo).FirstOrDefaultAsync();
                        if (lastActivity != null)
                        {
                            var lastActivityNo = lastActivity.ActivitiesNo;
                            var lastno = int.Parse(lastActivityNo.Substring(3));

                            uniqueActivityNo = "AC-" + (lastno + 1).ToString();
                        }
                        else
                        {
                            uniqueActivityNo = "AC-1";
                        }

                        var newActivity = new Activity();
                        newActivity.CreatedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        newActivity.UpdatedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        newActivity.Subject = activity.Subject;
                        newActivity.ActivitiesNo = uniqueActivityNo;
                        newActivity.Description = activity.Description;
                        newActivity.Status = activity.Status;
                        newActivity.UserId = activity.UserId;

                        _toDoContext.Activities.Add(newActivity);
                        await _toDoContext.SaveChangesAsync();

                        await transaction.CommitAsync();

                        _utility.WriteToFile(this.Request.Method, this.Request.GetDisplayUrl(), JsonConvert.SerializeObject(activity), JsonConvert.SerializeObject(newActivity), HttpStatusCode.OK.ToString());
                        return Ok(newActivity);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();

                        _utility.WriteToFile(this.Request.Method, this.Request.GetDisplayUrl(), JsonConvert.SerializeObject(activity), JsonConvert.SerializeObject(ex.Message), HttpStatusCode.InternalServerError.ToString());
                        return StatusCode(500, ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                _utility.WriteToFile(this.Request.Method, this.Request.GetDisplayUrl(), JsonConvert.SerializeObject(activity), JsonConvert.SerializeObject(ex.Message), HttpStatusCode.InternalServerError.ToString());
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPost("DeleteActivities")]
        public async Task<ActionResult> DeleteActivities([FromBody] Activity activity)
        {
            try
            {
                var activityExist = await _toDoContext.Activities.Where(x => x.ActivitiesNo == activity.ActivitiesNo).FirstOrDefaultAsync();
                if (activityExist != null)
                {
                    _toDoContext.Activities.Remove(activityExist);
                    await _toDoContext.SaveChangesAsync();

                    _utility.WriteToFile(this.Request.Method, this.Request.GetDisplayUrl(), JsonConvert.SerializeObject(activity), "", HttpStatusCode.OK.ToString());
                    return Ok(activity);
                }
                else
                {
                    string message = "Activities No not exist";
                    _utility.WriteToFile(this.Request.Method, this.Request.GetDisplayUrl(), JsonConvert.SerializeObject(activity), JsonConvert.SerializeObject(message), HttpStatusCode.BadRequest.ToString());
                    return BadRequest(message);
                }
            }
            catch (Exception ex)
            {
                _utility.WriteToFile(this.Request.Method, this.Request.GetDisplayUrl(), JsonConvert.SerializeObject(activity), JsonConvert.SerializeObject(ex.Message), HttpStatusCode.InternalServerError.ToString());
                return StatusCode(500, ex.Message);
            }
        }
    }
}
