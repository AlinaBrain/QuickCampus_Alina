﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickCampus_Core.Common;
using QuickCampus_Core.Interfaces;
using QuickCampus_Core.ViewModel;

namespace QuickCampusAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserRepo userRepo;
        private readonly IClientRepo clientRepo;
        private IConfiguration config;
        public UserController(IUserRepo userRepo, IClientRepo clientRepo, IConfiguration config)
        {
            this.userRepo = userRepo;
            this.clientRepo = clientRepo;
            this.config = config;
        }

        [Route("AddUser")]
        [HttpPost]
        public async Task<IActionResult> AddUser(UserModel vm)
        {
            vm.Password = EncodePasswordToBase64(vm.Password);
            IGeneralResult<UserVm> result = new GeneralResult<UserVm>();
            var _jwtSecretKey = config["Jwt:Key"];
            if (userRepo.Any(x => x.Email == vm.Email && x.IsActive == true && x.IsDelete == false))
            {
                result.Message = "Email Already Registered!";
            }
            else
            {
                if (ModelState.IsValid)
                {
                    // Decode the JWT token and retrieve the "id" claim

                    var clientId = JwtHelper.GetUserIdFromToken(Request.Headers["Authorization"], _jwtSecretKey);

                    //var clientId = vm.ClientId.HasValue ? await clientRepo.GetById((int)vm.ClientId) : null;

                    if (!string.IsNullOrEmpty(clientId))
                    {
                        int parsedClientId;
                        if (int.TryParse(clientId, out parsedClientId))
                        {
                            UserVm userVm = new UserVm
                            {
                                UserName = vm.Email,
                                Name = vm.Name,
                                Email = vm.Email,
                                Mobile = vm.Mobile,
                                Password = vm.Password,
                                ClientId = parsedClientId,
                                IsActive = true,
                                IsDelete = false
                            };
                            await userRepo.Add(userVm.toUserDbModel());
                            result.IsSuccess = true;
                            result.Message = "User added successfully.";
                            result.Data = userVm;
                            return Ok(result);
                        }
                        else
                        {
                            result.Message = "Invalid Client ID format.";
                        }
                    }
                    else
                    {
                        UserVm userVm = new UserVm
                        {
                            UserName = vm.Email,
                            Name = vm.Name,
                            Email = vm.Email,
                            Mobile = vm.Mobile,
                            Password = vm.Password,
                            ClientId = 0, 
                            IsActive = true,
                            IsDelete = false
                        };

                        await userRepo.Add(userVm.toUserDbModel());
                        result.IsSuccess = true;
                        result.Message = "User added successfully.";
                        result.Data = userVm;
                        return Ok(result);
                    }

                }
                else
                {
                    result.Message = GetErrorListFromModelState.GetErrorList(ModelState);
                }
            }

            return Ok(result);
        
    }

        [HttpGet]
        [Route("userList")]
        public async Task<IActionResult> userList()
        {

            var _jwtSecretKey = config["Jwt:Key"];
            var clientId = JwtHelper.GetClientIdFromToken(Request.Headers["Authorization"], _jwtSecretKey);
            IGeneralResult<List<UserResponseVm>> result = new GeneralResult<List<UserResponseVm>>();
            try
            {
                var categoryList = (await userRepo.GetAll()).Where(x => x.IsDelete == false || x.IsDelete == null).ToList();
                var res = categoryList.Select(x => ((UserResponseVm)x)).ToList();
                if (res != null)
                {
                    result.IsSuccess = true;
                    result.Message = "ClientList";
                    result.Data = res;
                }
                else
                {
                    result.Message = "Client List Not Found";
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }
            return Ok(result);
        }
        [HttpGet]
        [Route("DeleteUser")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var _jwtSecretKey = config["Jwt:Key"];
            var clientId = JwtHelper.GetClientIdFromToken(Request.Headers["Authorization"], _jwtSecretKey);
            IGeneralResult<UserVm> result = new GeneralResult<UserVm>();
            var res = await userRepo.GetById(id);
            if (res.IsDelete == false)
            {

                res.IsActive = false;
                res.IsDelete = true;
                await userRepo.Update(res);
                result.IsSuccess = true;
                result.Message = "USer Deleted Succesfully";
            }
            else
            {
                result.Message = "User does Not exist";
            }
            return Ok(result);
        }
        [HttpPost]
        [Route("EditUser")]
        public async Task<IActionResult> EditUser( UserVm vm)
        {
            IGeneralResult<UserResponseVm> result = new GeneralResult<UserResponseVm>();
            var _jwtSecretKey = config["Jwt:Key"];
            var userId = JwtHelper.GetIdFromToken(Request.Headers["Authorization"], _jwtSecretKey);
            if (userRepo.Any(x => x.Email == vm.Email && x.IsActive == true && x.Id != vm.Id))
            {
                result.Message = "Email Already Registered!";
            }
            else if (userRepo.Any(x => x.IsActive == true && x.Name == vm.Name.Trim()))
            {
                result.Message = "UserName Already Exist!";
            }
            else
            {
                var res = await userRepo.GetById(vm.Id);
                bool isDeleted = (bool)res.IsDelete ? true : false;
                if (isDeleted)
                {
                    result.Message = " User does Not Exist";
                    return Ok(result);
                }

                if (ModelState.IsValid && vm.Id > 0 && res.IsDelete == false)
                {


                    UserVm userVm = new UserVm
                    {
                        Id = vm.Id,
                        Name = vm.Name,
                        Email = vm.Email,
                        Mobile = vm.Mobile,
                        

                        
                    };
                    try
                    {
                        var TblClien = userVm.ToUpdateDbModel();
                        result.Data = (UserResponseVm)await userRepo.Update(userVm.ToUpdateDbModel());
                        result.Message = "Client updated successfully";
                        result.IsSuccess = true;
                    }
                    catch (Exception ex)
                    {
                        result.Message = ex.Message;
                    }
                    return Ok(result);
                }
                else
                {
                    result.Message = "something Went Wrong";
                }

            }
            return Ok(result);
        }

        [HttpGet]
        [Route("activeAndInactive")]
        public async Task<IActionResult> activeAndInactive(bool IsActive, int id)
        {
            IGeneralResult<dynamic> result = new GeneralResult<dynamic>();
            if (id > 0)
            {
                var res = await userRepo.GetById(id);
                if (res != null)
                {
                    res.IsActive = IsActive;
                    await userRepo.Update(res);
                    result.IsSuccess = true;
                    result.Message = "Your status is changed successfully";
                    result.Data = res;
                    return Ok(result);
                }
                else
                {
                    result.Message = GetErrorListFromModelState.GetErrorList(ModelState);
                }
            }
            return Ok(result);
        }
        private string EncodePasswordToBase64(string password)
        {
            try
            {
                byte[] encData_byte = new byte[password.Length];
                encData_byte = System.Text.Encoding.UTF8.GetBytes(password);
                string encodedData = Convert.ToBase64String(encData_byte);
                return encodedData;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in base64Encode" + ex.Message);
            }
        }
    }
}
