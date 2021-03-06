﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

using TokenGenerationService.Common;
using TokenGenerationService.Models;
using TokenGenerationService.Services;

//using Microsoft.AspNetCore.Authorization;

namespace TokenGenerationService.Controllers
{
    [Route("api/[controller]s")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        /// <summary>
        ///     Private read-only service to perform CRUD operations on the database.
        /// </summary>
        private readonly TokenService _tokenService;

        /// <summary>
        ///     Creates a token service to go along with the token controller.
        /// </summary>
        /// <param name="tokenService">
        ///     The token service assigned to this token controller.
        /// </param>
        public TokenController(TokenService tokenService)
        {
            _tokenService = tokenService;
        }

        /// <summary>
        ///     Returns all tokens currently in the database.
        /// </summary>
        /// <returns>
        ///     A list of all tokens. (May be empty list)
        /// </returns>
        [HttpGet]
        public ActionResult<List<TokenTime>> GetAll()
        {
            List<Token> tokens;
            try
            {
                tokens = _tokenService.Get();
            }
            catch
            {
                tokens = new List<Token>();
            }

            return tokens.Select(t => Utils.ToTokenTime(t)).Cast<TokenTime>().ToList();
        }
        
        /// <summary>
        ///     Returns one token matching the tokenString specified in the "data".
        /// </summary>
        /// <param name="data">
        ///     GET request payload
        /// </param>
        /// <returns>
        ///     Returns NotFound if no matching token is found in the database. 
        ///     Returns one token object if the token exists in the database. 
        /// </returns>
        [HttpGet("{tokenString:length(20)}", Name = "GetToken")]
        public ActionResult<TokenTime> GetOne(string tokenString)
        {
            Token token = _tokenService.Get(tokenString);
            if (token == null)
            {
                return NotFound();
            }

            return Utils.ToTokenTime(token);
        }
        
        /// <summary>
        ///     Default token creation.
        ///     Creates a token with 7 day expiration.
        /// </summary>
        /// <returns>
        ///     Returns the token which was just created.
        /// </returns>
        [HttpPost("default/{email}")]
        public object CreateDefault([FromRoute]string email)
        {
            Token token = _tokenService.Create(email, Utils.DEFAULT_TIME);
            return new TokenTime { tokenString = token.TokenString, time = Utils.TimeRemaining(token.created, token.ttl) };
        }

        /// <summary>
        ///     Creates a new token with a valid duration specified in "data".
        /// </summary>
        /// <param name="data">
        ///     POST request payload containing a specified number of days.
        /// </param>
        /// <returns>
        ///     Returns the token which was just created.
        /// </returns>
        [HttpPost]
        public object Create([FromBody]EmailTime emailTime)
        {
            if (!Utils.MeetsMinimumTimeRequirement(emailTime.time))
            {
                return BadRequest();
            }

            Token token = _tokenService.Create(emailTime.email, emailTime.time);

            return new TokenTime { tokenString = token.TokenString, time = Utils.TimeRemaining(token.created, token.ttl) };
        }

        /// <summary>
        ///     Modifies a token's expiry date. Converts "Time" to "TTL" and adds to 'ttl'
        /// </summary>
        /// <param name="data">
        ///     PUT request payload containing the tokenString of the token
        ///     to be modified and a specified number of time for extension.
        /// </param>
        /// <returns>
        ///     Returns NotFound if no matching token is found in the database. 
        ///     Returns OkResult if token was found and time was updated.
        /// </returns>
        [HttpPut]
        public IActionResult ExtendTime([FromBody]TokenTime data)
        {
            Token token = _tokenService.Get(data.tokenString);
            if (token == null)
            {
                return NotFound();
            }

            Token newToken = _tokenService.ExtendTime(data);
            return Ok(Utils.ToTokenTime(newToken));
        }

        /// <summary>
        ///     Validates a token. 
        ///     If the validation time is before the token's expiry date, then the token is validated.
        ///     Else the current time is past the token's expiry date and token is not validated.
        ///     The token is removed from the database after attempting to validate it.
        /// </summary>
        /// <param name="data">
        ///     PATCH request payload containing the tokenString of the token to be validated.
        /// </param>
        /// <returns>
        ///     Returns NotFound if no matching token is found in the database. 
        ///     Return Ok if the token IS validated.
        /// </returns>
        //[HttpPatch("{tokenString}")]
        [HttpPost("validate/{tokenString}")]
        public object Validate(string tokenString)
        {
            Token token = _tokenService.Get(tokenString);
            if (token != null && _tokenService.Validate(token.TokenString))
            {
                return ( new { email = token.email, validated = true } );
            }

            return ( new { email = token.email, validated = false } );
        }

        /// <summary>
        ///     Here temporarily for testing purposes.
        ///     Quickly clear the database of tokens.
        /// </summary>
        /// <returns>
        ///     Returns OkResult.
        /// </returns>
        [HttpDelete]
        public IActionResult Delete()
        {
            _tokenService.Remove();
            return new OkResult();
        }
    }
}
