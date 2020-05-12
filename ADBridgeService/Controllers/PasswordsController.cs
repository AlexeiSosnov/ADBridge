using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ADBridgeService.AppServices;
using ADBridgeService.Contracts;
using ADClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace ADBridgeService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PasswordsController : ControllerBase
    {
        public PasswordsController(
            IGetPasswordExpirationService passwordExpirationService
        )
        {
            this.passwordExpirationService = passwordExpirationService;
        }

        [HttpGet("ExpirationDate")]
        [ResponseCache(NoStore = true )]
        public ActionResult<PasswordExpirationDTO> GetAccountExpirationDate([FromQuery]string account)
        {
            try
            {
                var resultDTO = this.passwordExpirationService.GetDomainUserPasswordExpirationDateOrNull(account);
                // если ничего не нашли, вернем стандартный код ответа с пояснением
                if (resultDTO == null)
                {
                    return NotFound(account);
                }

                return resultDTO;
            }
            catch (ConfigurationException)
            {
                // ошибка конфигурации -- это фатальная ошибка сервера, вернем 500
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ErrorDTO("bad AD connection configuration")
                );
            }
            catch (ConnectionException)
            {
                // ошибка подключения к AD -- это bad gateway, см. https://developer.mozilla.org/ru/docs/Web/HTTP/Status/502
                return StatusCode(
                    StatusCodes.Status502BadGateway,
                    new ErrorDTO("cannot connect to AD storage")
                );
            }
            // другие ошибки пусть валятся, как есть 
        }

        private readonly IGetPasswordExpirationService passwordExpirationService;
    }
}