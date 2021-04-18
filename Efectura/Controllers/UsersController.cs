using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Efectura.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly EfecturaDbContext dbContext;

        public UsersController(EfecturaDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpPost]
        public IActionResult Add([FromBody] JsonElement data)
        {
            try
            {
                string name = data.GetProperty("name").GetString();
                string surname = data.GetProperty("surname").GetString();
                DateTime birthday = data.GetProperty("birthday").GetDateTime();
                JsonElement addressJson;
                string address = null;
                if (data.TryGetProperty("address", out addressJson))
                {
                    address = addressJson.GetString();
                }
                string TCKN;
                User user;
                while (true)
                {
                    TCKN = TCKNGenerator.Random();
                    if (dbContext.User.Find(TCKN) == null)
                    {
                        user = new User { 
                            TCKN = TCKN,
                            Name = name,
                            Surname = surname, 
                            Birthday = birthday, 
                            Address = address 
                        };
                        dbContext.Add(user);
                        break;
                    }
                }
                dbContext.SaveChanges();
                return Created("/api/Users/" + TCKN, user);
            }
            catch (Exception)
            {
                return Ok(Ok().StatusCode);
            }
        }

        [HttpPut("{TCKN}")]
        public IActionResult Update(string TCKN, [FromBody] JsonElement data)
        {
            try
            {
                JsonElement json;
                string name = null;
                string surname = null;
                DateTime birthday = DateTime.MinValue;
                User user = dbContext.User.Find(TCKN);

                if (data.TryGetProperty("name", out json))
                {
                    name = json.GetString();
                }

                if (data.TryGetProperty("surname", out json))
                {
                    surname = json.GetString();
                }

                if (data.TryGetProperty("birthday", out json))
                {
                    birthday = json.GetDateTime();
                }
                string address = null;
                if (data.TryGetProperty("address", out json))
                {
                    address = json.GetString();
                }

                if (user == null)
                {
                    return BadRequest("No such user.");
                }
                if (name != null)
                {
                    user.Name = name;
                }
                if (surname != null)
                {
                    user.Surname = surname;
                }
                if (birthday != DateTime.MinValue)
                {
                    user.Birthday = birthday;
                }
                if (address != null)
                {
                    user.Address = address;
                }
                if (name != null || surname != null || birthday != DateTime.MinValue || address != null)
                {
                    user.ModifiedDate = DateTime.Now;
                }
                else
                {
                    return BadRequest("Not a valid object.");
                }
                dbContext.SaveChanges();
                return Ok(TCKN + " was updated.");
            }
            catch (Exception)
            {
                return Ok(Ok().StatusCode);
            }
        }

        [HttpGet]
        public IActionResult List()
        {
            try
            {
                return Ok(dbContext.User.ToList());
            }
            catch (Exception)
            {
                return Ok(Ok().StatusCode);
            }
        }

        [HttpDelete("{TCKN}")]
        public IActionResult Delete(string TCKN)
        {
            try
            {
                User user = dbContext.User.Find(TCKN);
                if (user == null)
                {
                    return BadRequest("No such user.");
                }
                dbContext.User.Remove(user);
                dbContext.SaveChanges();
                return Ok(TCKN + " was deleted.");
            }
            catch (Exception)
            {

                return Ok(Ok().StatusCode);
            }
        }
    }
}
