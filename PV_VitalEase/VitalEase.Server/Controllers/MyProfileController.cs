using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using VitalEase.Server.Data;
using VitalEase.Server.ViewModel;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VitalEase.Server.Models;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace VitalEase.Server.Controllers
{
    /// <summary>
    /// Controller responsible for managing the user's profile.
    /// </summary>
    public class MyProfileController : Controller
    {
        /// <summary>
        /// VitalEaseServerContext database context, used to access the application's data.
        /// </summary>
        private readonly VitalEaseServerContext _context;

        /// <summary>
        /// Configuration interface, used to access the application's settings.
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyProfileController"/> controller.
        /// </summary>
        /// <param name="context">
        /// The database context (<see cref="VitalEaseServerContext"/>) that enables data access operations.
        /// </param>
        /// <param name="configuration">
        /// The configuration interface (<see cref="IConfiguration"/>) used to access the application's settings.
        /// </param>
        public MyProfileController(VitalEaseServerContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// Deletes a user's account based on the provided email.
        /// </summary>
        /// <param name="email">
        /// The email address of the user whose account is to be deleted.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> indicating:
        /// <list type="bullet">
        ///   <item>
        ///     An <c>Ok</c> result with a success message if the account is successfully deleted.
        ///   </item>
        ///   <item>
        ///     A <c>BadRequest</c> result if the email is not provided or if an error occurs during the process.
        ///   </item>
        ///   <item>
        ///     A <c>NotFound</c> result if no user is found with the specified email.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method performs the following operations:
        /// <list type="bullet">
        ///   <item>
        ///     Checks if the email has been provided; if not, it returns a <c>BadRequest</c> with an appropriate message.
        ///   </item>
        ///   <item>
        ///     Uses <c>Uri.UnescapeDataString</c> to ensure that the email is correctly decoded.
        ///   </item>
        ///   <item>
        ///     Searches the database for a user with the provided email, including their associated profile.
        ///   </item>
        ///   <item>
        ///     If the user is not found, it returns a <c>NotFound</c> with the message "User not found".
        ///   </item>
        ///   <item>
        ///     Sends an email notification informing that the account has been deleted; if the sending fails, logs the action and returns a server error.
        ///   </item>
        ///   <item>
        ///     If the user has an associated profile, it is removed before the user itself is deleted.
        ///   </item>
        ///   <item>
        ///     Finally, deletes the user and saves the changes to the database, returning an <c>Ok</c> result with a success message.
        ///   </item>
        /// </list>
        /// </remarks>
        [HttpDelete("api/deleteAccount")]
        public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountConfirmationViewModel model)
        {
            try
            {

                var (isValid, email, userId, tokenId) = ValidateDeleteAccountToken(model.Token);

                if (!isValid)
                {
                    await LogAction("Email change Attempt", "Failed - Error changing Email.", 0);
                    return BadRequest(new { message = "Token expired." });
                }

                if (!ModelState.IsValid)
                {
                    // Registrar log de erro (dados inválidos)
                    await LogAction("Delete Account Attempt", "Failed - Invalid Data", 0);
                    return BadRequest(new { message = "Invalid data" }); // Retorna erro 400
                }

                email = Uri.UnescapeDataString(model.Email);

                var user = await _context.Users
                    .Include(u => u.Profile) // Garante que o perfil também é carregado
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                var deleteAccountToken = await _context.DeleteAccountTokens.FirstOrDefaultAsync(u => u.TokenId == tokenId);

                if (deleteAccountToken != null && deleteAccountToken.IsUsed == false)
                {

                    deleteAccountToken.IsUsed = true;
                    _context.DeleteAccountTokens.Remove(deleteAccountToken);
                }
                else
                {
                    await LogAction("Delete Account Attempt", "Failed - Delete Account token is null.", userId);
                    return BadRequest(new { message = "Delete Account token is null." });
                }

                var deleteAccountNotification = await
                SendEmail(
                 user.Email,
                 "VitalEase - Delete account Notification",
                 "All your data has been successfully deleted. Thank you for using our App."
               );

                if (!deleteAccountNotification)
                {
                    await LogAction("Delete Account Attempt", "Failed - Failed to send delete account notification", user.Id);
                    return StatusCode(500, new { message = "Failed to send delete account notification to email." });
                }

                if (user.Profile != null)
                {
                    _context.Profiles.Remove(user.Profile); // Remove o perfil corretamente
                }

                _context.Users.Remove(user); // Remove o utilizador
                await _context.SaveChangesAsync();

                return Ok(new { message = "Account deleted successfully" });
            }
            catch (Exception ex)
            {
                await LogAction("Delete Account Attempt", "Failed - Error deleting account", 0);
                return BadRequest(new { message = "Error deleting account", error = ex.Message });
            }
        }

        /// <summary>
        /// Cancels the new email change request by removing the associated reset email token 
        /// and sending a cancellation notification to the user's old email address.
        /// </summary>
        /// <param name="model">
        /// A <see cref="ConfirmNewEmailViewModel"/> containing the token used for validating 
        /// the email change request.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> that indicates:
        /// <list type="bullet">
        ///   <item>
        ///     <c>Ok</c> with a success message and the new email if the cancellation is successful.
        ///   </item>
        ///   <item>
        ///     <c>BadRequest</c> if the token is expired or if there is an error sending the cancellation notification.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// The method performs the following operations:
        /// <list type="bullet">
        ///   <item>
        ///     Validates the token using the <c>ValidateToken</c> method, which returns a tuple containing the validity,
        ///     the old email, the new email, the user ID, and the token ID.
        ///   </item>
        ///   <item>
        ///     If the token is not valid, logs the attempt and returns a <c>BadRequest</c> with the message "Token expired."
        ///   </item>
        ///   <item>
        ///     Retrieves the reset email token from the database using the token ID, and if found, removes it.
        ///   </item>
        ///   <item>
        ///     Sends a cancellation notification email to the old email address.
        ///   </item>
        ///   <item>
        ///     Logs the cancellation of the email change attempt, saves changes to the database,
        ///     and returns an <c>Ok</c> response with a success message and the new email.
        ///   </item>
        /// </list>
        /// </remarks>
        [HttpPost("api/deleteAccountCancellation")]
        public async Task<IActionResult> DeleteAccountCancellation([FromBody] DeleteAccountCancellationViewModel model)
        {
            var (isValid, email, userId, tokenId) = ValidateDeleteAccountToken(model.Token);

            if (!isValid)
            {
                await LogAction("Delete Account Attempt", "Failed - Token Expired.", 0);
                return BadRequest(new { message = "Token expired." });
            }

            var deleteAccountToken = await _context.DeleteAccountTokens.FirstOrDefaultAsync(u => u.TokenId == tokenId);

            if (deleteAccountToken != null)
            {
                _context.DeleteAccountTokens.Remove(deleteAccountToken);
            }


            var cancelNotification = await
                SendEmail(
                    email,
                    "VitalEase - Delete Account Request",
                    "The delete account request has been canceled."
                );

            if (!cancelNotification)
            {
                await LogAction("Delete Account Attempt", "Failed - Delete Account canceled email has not been sended.", userId);
                return StatusCode(500, new { message = "Failed to send cancel notification email to email." });
            }

            await LogAction("Delete Account Attempt", "Success - Delete account successfully canceled.", userId);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Delete account successfully canceled." });
        }

        /// <summary>
        /// Processes a request to delete a user account by sending an email containing a confirmation link.
        /// </summary>
        /// <param name="model">
        /// An instance of <see cref="DeleteAccountRequestViewModel"/> containing the email address of the user requesting account deletion.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> indicating the outcome of the operation, which may be:
        /// <list type="bullet">
        ///   <item>
        ///     <description>
        ///       <c>Ok</c> – A confirmation email for account deletion has been successfully sent.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <description>
        ///       <c>BadRequest</c> – The provided data is invalid, or an error occurred during processing.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <description>
        ///       <c>Unauthorized</c> – No user was found matching the provided email address.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <description>
        ///       <c>StatusCode(500)</c> – The email containing the deletion link could not be sent.
        ///     </description>
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method performs the following steps:
        /// <list type="bullet">
        ///   <item>
        ///     <description>
        ///       Validates the input data; if the model state is invalid, logs the error and returns a <c>BadRequest</c>.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <description>
        ///       Searches the database for a user with the provided email address.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <description>
        ///       If no user is found, logs the attempt and returns an <c>Unauthorized</c> result.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <description>
        ///       If a user is found, generates an account deletion token using the user's email and ID.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <description>
        ///       Constructs a confirmation link that includes the generated token as a query parameter.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <description>
        ///       Sends an email containing the delete account confirmation link to the user's email address.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <description>
        ///       If the email fails to send, logs the error and returns a server error (<c>StatusCode(500)</c>).
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <description>
        ///       If the email is successfully sent, logs the successful operation and returns an <c>Ok</c> result with a success message.
        ///     </description>
        ///   </item>
        /// </list>
        /// </remarks>
        [HttpPost("api/deleteAccountRequest")]
        public async Task<IActionResult> DeleteAccountResquest([FromBody] DeleteAccountRequestViewModel model)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    // Registrar log de erro (dados inválidos)
                    await LogAction("Email change Attempt", "Failed - Invalid Data", 0);
                    return BadRequest(new { message = "Invalid data" }); // Retorna erro 400
                }


                var user = await _context.Users
                   .FirstOrDefaultAsync(u => u.Email == model.Email);


                if (user == null)
                {
                    await LogAction("Delete Account Attempt", "Failed - No user found", 0);
                    return Unauthorized(new { message = "No user found" });
                }

                var token = GenerateDeleteAccountToken(user.Email, user.Id);

                var deleteAccountLink = $"https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/deleteAccountConfirmation?token={Uri.EscapeDataString(token)}";

                var emailSent = await SendDeleteAccountLink(model.Email, deleteAccountLink);
                if (!emailSent)
                {
                    await LogAction("Delete Account Attempt", "Failed - Failed to send delete account request to email.", user.Id);
                    return StatusCode(500, new { message = "Failed to send delete account request to email." });
                }


                await LogAction("Delete Account Attempt", "Success - Email to delete your account has been sent to your email.", user.Id);
                return Ok(new { message = "Email to delete your account has been sent to your email." });
            }
            catch (Exception ex)
            {
                await LogAction("Delete Account Attempt", "Failed - Error requesting delete account.", 0);
                return BadRequest(new { message = "Error requesting delete account", error = ex.Message });
            }
        }

        /// <summary>
        /// Validates the provided password for a given user.
        /// </summary>
        /// <param name="request">
        /// An object of type <see cref="PasswordValidationRequest"/> that contains the email and the password to be validated.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> indicating:
        /// <list type="bullet">
        ///   <item>
        ///     <c>Ok</c> with a success message if the password is valid.
        ///   </item>
        ///   <item>
        ///     <c>BadRequest</c> if the email or password is not provided.
        ///   </item>
        ///   <item>
        ///     <c>NotFound</c> if the user is not found.
        ///   </item>
        ///   <item>
        ///     <c>Unauthorized</c> if the password is invalid.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// The method performs the following operations:
        /// <list type="bullet">
        ///   <item>
        ///     Checks if the email and password have been provided.
        ///   </item>
        ///   <item>
        ///     Searches for the user in the database based on the email.
        ///   </item>
        ///   <item>
        ///     Validates the password using the <c>VerifyPassword</c> method to compare the provided password with the stored one (typically using a hash).
        ///   </item>
        ///   <item>
        ///     Returns an <c>Ok</c> with a message if the validation is successful, or the corresponding error otherwise.
        ///   </item>
        /// </list>
        /// </remarks>
        [HttpPost("api/validatePassword")]
        public async Task<IActionResult> ValidatePassword([FromBody] PasswordValidationRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "Email and password are required." });
            }

            // Retrieve the user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Validate password (you might need to implement password hashing check here)
            var passwordIsValid = VerifyPassword(request.Password, user.Password);
            if (!passwordIsValid)
            {
                return Unauthorized(new { message = "Invalid password." });
            }

            return Ok(new { message = "Password is valid." });
        }

        /// <summary>
        /// Checks whether the provided password matches the stored password hash.
        /// </summary>
        /// <param name="password">The plain text password to be verified.</param>
        /// <param name="hashedPassword">The password hash to be compared against.</param>
        /// <returns>
        /// Returns <c>true</c> if the password, once hashed, matches the stored hash; otherwise, <c>false</c>.
        /// </returns>
        private bool VerifyPassword(string password, string hashedPassword)
        {
            return HashPassword(password) == hashedPassword ? true : false;
        }

        /// <summary>
        /// Retrieves the profile information for a user based on the provided email.
        /// </summary>
        /// <param name="email">
        /// The email address of the user whose profile information is to be retrieved.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> that contains:
        /// <list type="bullet">
        ///   <item>
        ///     an <c>Ok</c> result with the profile data (username, birthdate, weight, height, gender, and indication of heart problems) if the user and profile are found;
        ///   </item>
        ///   <item>
        ///     a <c>BadRequest</c> result if the email is not provided or if an error occurs during processing;
        ///   </item>
        ///   <item>
        ///     a <c>NotFound</c> result if the user or profile is not found.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// The method performs the following operations:
        /// <list type="bullet">
        ///   <item>
        ///     Checks whether the email is provided and decodes it if necessary.
        ///   </item>
        ///   <item>
        ///     Searches the database for the user by email, including the associated profile (using <c>Include(u => u.Profile)</c>).
        ///   </item>
        ///   <item>
        ///     If the user or profile is not found, it returns a <c>NotFound</c> result with an appropriate message.
        ///   </item>
        ///   <item>
        ///     If found, it maps the profile data to an anonymous object and returns it in an <c>Ok</c> result.
        ///   </item>
        /// </list>
        /// </remarks>
        [HttpGet("api/getProfileInfo/{email}")]
        public async Task<IActionResult> GetProfileInfo(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    return BadRequest(new { message = "Email is required" });
                }

                email = Uri.UnescapeDataString(email);

                // Busca o perfil do usuário pelo email e inclui o perfil
                var userProfile = await _context.Users
                    .Include(u => u.Profile) // <- Isso garante que o Profile seja carregado
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (userProfile == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                if (userProfile.Profile == null)
                {
                    return NotFound(new { message = "Profile not found" });
                }

                var data = new
                {
                    username = userProfile.Profile.Username,
                    birthDate = userProfile.Profile.Birthdate.ToString("yyyy-MM-dd"),
                    weight = userProfile.Profile.Weight,
                    height = userProfile.Profile.Height,
                    gender = userProfile.Profile.Gender,
                    hasHeartProblems = userProfile.Profile.HasHeartProblems,
                };

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error fetching profile", error = ex.Message });
            }
        }

        /// <summary>
        /// Changes the birthdate registered in a user's profile.
        /// </summary>
        /// <param name="model">
        /// An object of type <see cref="ChangeBirthDateViewModel"/> that contains the user's email and the new birthdate.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> that indicates:
        /// <list type="bullet">
        ///   <item>
        ///     an <c>Ok</c> response with a success message if the birthdate is changed successfully;
        ///   </item>
        ///   <item>
        ///     a <c>BadRequest</c> response if the submitted data is invalid or if the user is younger than 16 years old;
        ///   </item>
        ///   <item>
        ///     an <c>Unauthorized</c> response if the user is not found;
        ///   </item>
        ///   <item>
        ///     a <c>NotFound</c> response if the user's profile is not found.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method performs the following operations:
        /// <list type="bullet">
        ///   <item>
        ///     It validates the submitted data. If the data is invalid, it logs the attempt and returns a <c>BadRequest</c>.
        ///   </item>
        ///   <item>
        ///     It searches for the user based on the provided email, including the associated profile (using <c>Include(u => u.Profile)</c>).
        ///   </item>
        ///   <item>
        ///     If the user or their profile is not found, it logs the attempt and returns an error (<c>Unauthorized</c> or <c>NotFound</c>).
        ///   </item>
        ///   <item>
        ///     It checks whether the new birthdate indicates that the user is at least 16 years old. If not, it logs the attempt and returns a <c>BadRequest</c>.
        ///   </item>
        ///   <item>
        ///     If all validations are satisfied, it updates the birthdate in the user's profile, saves the changes, and logs the successful operation.
        ///   </item>
        /// </list>
        /// </remarks>
        [HttpPost("api/changeBirthDate")]
        public async Task<IActionResult> ChangeBirthDate([FromBody] ChangeBirthDateViewModel model)
        {
            try {

                var userProfile = await _context.Users
                .Include(u => u.Profile) 
                   .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (userProfile == null)
                {
                    await LogAction("Birth date change Attempt", "Failed - No user found", 0);
                    return Unauthorized(new { message = "No user found" });
                }

                if (userProfile.Profile == null)
                {
                    await LogAction("Birth date change Attempt", "Failed - Profile not found", userProfile.Id);
                    return NotFound(new { message = "Profile not found" });
                }

                if (model.BirthDate > DateTime.Today.AddYears(-16))
                {
                    await LogAction("Birth date change Attempt", "Failed - Age restriction", userProfile.Id);
                    return BadRequest(new { message = "You must be at least 16 years old." });
                }

                userProfile.Profile.Birthdate = model.BirthDate;
                await _context.SaveChangesAsync();

                await LogAction("Birth Date change Attempt", "Success - Birth Date changed successfully.", 0);
                return Ok(new {message = "Birth date changed successfully!!"});
            }
            catch (Exception ex) {
                return BadRequest(new { message = "Error changing birth date", error = ex.Message });
            }
        }

        /// <summary>
        /// Changes the weight registered in a user's profile.
        /// </summary>
        /// <param name="model">
        /// An object of type <see cref="ChangeWeightViewModel"/> that contains the user's email and the new weight.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> that indicates:
        /// <list type="bullet">
        ///   <item>
        ///     an <c>Ok</c> response with a success message if the weight is updated successfully;
        ///   </item>
        ///   <item>
        ///     a <c>BadRequest</c> response if the submitted data is invalid or if an error occurs during processing;
        ///   </item>
        ///   <item>
        ///     an <c>Unauthorized</c> response if no user is found with the provided email;
        ///   </item>
        ///   <item>
        ///     a <c>NotFound</c> response if the user's profile is not found.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method performs the following operations:
        /// <list type="bullet">
        ///   <item>
        ///     Validates the submitted data. If the data is invalid, it logs the update attempt and returns a <c>BadRequest</c>.
        ///   </item>
        ///   <item>
        ///     Searches the database for a user using the provided email, including their associated profile.
        ///   </item>
        ///   <item>
        ///     If the user or the user's profile is not found, it logs the attempt and returns the appropriate response.
        ///   </item>
        ///   <item>
        ///     Updates the weight field in the user's profile with the new value and saves the changes to the database.
        ///   </item>
        ///   <item>
        ///     Logs the successful operation and returns an <c>Ok</c> response with a success message.
        ///   </item>
        /// </list>
        /// </remarks>
        [HttpPost("api/changeWeight")]
        public async Task<IActionResult> ChangeWeight([FromBody] ChangeWeightViewModel model)
        {
            try
            {
                

                var userProfile = await _context.Users
                .Include(u => u.Profile)
                   .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (userProfile == null)
                {
                    await LogAction("Weight change Attempt", "Failed - No user found", 0);
                    return Unauthorized(new { message = "No user found" });
                }

                if (userProfile.Profile == null)
                {
                    await LogAction("Weight change Attempt", "Failed - Profile not found", userProfile.Id);
                    return NotFound(new { message = "Profile not found" });
                }

                if (model.Weight > 400 || model.Weight < 30)
                {
                    await LogAction("Weight change Attempt", "Weight must be between 30 and 400 kg.", userProfile.Id);
                    return Unauthorized(new { message = "Weight must be between 30 and 400 kg." });
                }

                userProfile.Profile.Weight = model.Weight;
                await _context.SaveChangesAsync();

                await LogAction("Weight change Attempt", "Success - Weight changed successfully.", userProfile.Id);
                return Ok(new { message = "Weight changed successfully!!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error changing Weight", error = ex.Message });
            }
        }

        /// <summary>
        /// Changes the height registered in a user's profile.
        /// </summary>
        /// <param name="model">
        /// An object of type <see cref="ChangeHeightViewModel"/> that contains the user's email and the new height.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> that indicates:
        /// <list type="bullet">
        ///   <item>
        ///     an <c>Ok</c> response with a success message if the height is updated successfully;
        ///   </item>
        ///   <item>
        ///     a <c>BadRequest</c> response if the submitted data is invalid or if an error occurs during processing;
        ///   </item>
        ///   <item>
        ///     an <c>Unauthorized</c> response if no user is found with the provided email;
        ///   </item>
        ///   <item>
        ///     a <c>NotFound</c> response if the user's profile is not found.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method performs the following operations:
        /// <list type="bullet">
        ///   <item>
        ///     Validates the submitted data. If the data is invalid, it logs the update attempt and returns a <c>BadRequest</c>.
        ///   </item>
        ///   <item>
        ///     Searches the database for a user using the provided email, including their associated profile (using <c>Include(u => u.Profile)</c>).
        ///   </item>
        ///   <item>
        ///     If the user or their profile is not found, it logs the attempt and returns the appropriate response.
        ///   </item>
        ///   <item>
        ///     Updates the height field in the user's profile with the provided value and saves the changes to the database.
        ///   </item>
        ///   <item>
        ///     Logs the successful operation and returns an <c>Ok</c> response with a success message.
        ///   </item>
        /// </list>
        /// </remarks>
        [HttpPost("api/changeHeight")]
        public async Task<IActionResult> ChangeHeight([FromBody] ChangeHeightViewModel model)
        {
            try
            {


                var userProfile = await _context.Users
                .Include(u => u.Profile)
                   .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (userProfile == null)
                {
                    await LogAction("Height change Attempt", "Failed - No user found", 0);
                    return Unauthorized(new { message = "No user found" });
                }

                if (userProfile.Profile == null)
                {
                    await LogAction("Height change Attempt", "Failed - Profile not found", userProfile.Id);
                    return NotFound(new { message = "Profile not found" });
                }

                if (model.Height > 251 || model.Height < 90 )
                {
                    await LogAction("Height change Attempt", "Failed - Height must be between 90 and 251 cm.", userProfile.Id);
                    return Unauthorized(new { message = "Height must be between 90 and 251 cm." });
                }

                userProfile.Profile.Height = model.Height;
                await _context.SaveChangesAsync();

                await LogAction("Height change Attempt", "Success - Height changed successfully.", userProfile.Id);
                return Ok(new { message = "Height changed successfully!!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error changing Height", error = ex.Message });
            }
        }

        /// <summary>
        /// Changes the gender registered in a user's profile.
        /// </summary>
        /// <param name="model">
        /// An object of type <see cref="ChangeGenderViewModel"/> that contains the user's email and the new gender.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> that indicates:
        /// <list type="bullet">
        ///   <item>
        ///     an <c>Ok</c> response with a success message if the gender is changed successfully;
        ///   </item>
        ///   <item>
        ///     a <c>BadRequest</c> response if the provided data is invalid or if an error occurs during processing;
        ///   </item>
        ///   <item>
        ///     an <c>Unauthorized</c> response if no user is found with the provided email;
        ///   </item>
        ///   <item>
        ///     a <c>NotFound</c> response if the user's profile is not found.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method performs the following operations:
        /// <list type="bullet">
        ///   <item>
        ///     Validates the submitted data. If the data is invalid, it logs the attempt and returns a <c>BadRequest</c>.
        ///   </item>
        ///   <item>
        ///     Searches the database for a user with the provided email, including their associated profile.
        ///   </item>
        ///   <item>
        ///     If the user or the user's profile is not found, it logs the attempt and returns the appropriate response.
        ///   </item>
        ///   <item>
        ///     Updates the gender field in the profile with the new value and saves the changes to the database.
        ///   </item>
        ///   <item>
        ///     Logs the successful operation and returns an <c>Ok</c> response with a success message.
        ///   </item>
        /// </list>
        /// </remarks>
        [HttpPost("api/changeGender")]
        public async Task<IActionResult> ChangeGender([FromBody] ChangeGenderViewModel model)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    // Registrar log de erro (dados inválidos)
                    await LogAction("Gender change Attempt", "Failed - Invalid Data", 0);
                    return BadRequest(new { message = "Invalid data" }); // Retorna erro 400
                }

                var userProfile = await _context.Users
                .Include(u => u.Profile)
                   .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (userProfile == null)
                {
                    await LogAction("Gender change Attempt", "Failed - No user found", 0);
                    return Unauthorized(new { message = "No user found" });
                }

                if (userProfile.Profile == null)
                {
                    await LogAction("Gender change Attempt", "Failed - Profile not found", userProfile.Id);
                    return NotFound(new { message = "Profile not found" });
                }


                userProfile.Profile.Gender = model.Gender;
                await _context.SaveChangesAsync();

                await LogAction("Gender change Attempt", "Success - Gender changed successfully.", userProfile.Id);
                return Ok(new { message = "Gender changed succesfully!!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error changing Gender", error = ex.Message });
            }
        }

        /// <summary>
        /// Changes the "HasHeartProblems" status registered in a user's profile.
        /// </summary>
        /// <param name="model">
        /// An object of type <see cref="ChangeHasHeartProblemsViewModel"/> that contains the user's email and the new value for "HasHeartProblems".
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> that indicates:
        /// <list type="bullet">
        ///   <item>
        ///     an <c>Ok</c> response with a success message if the value is updated successfully;
        ///   </item>
        ///   <item>
        ///     a <c>BadRequest</c> response if the provided data is invalid or an error occurs during processing;
        ///   </item>
        ///   <item>
        ///     an <c>Unauthorized</c> response if no user is found with the provided email;
        ///   </item>
        ///   <item>
        ///     a <c>NotFound</c> response if the user's profile is not found.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// The method performs the following operations:
        /// <list type="bullet">
        ///   <item>
        ///     It validates the submitted data. If the data is invalid, it logs the attempt and returns a <c>BadRequest</c>.
        ///   </item>
        ///   <item>
        ///     It searches the database for a user using the provided email, including the associated profile (using <c>Include(u => u.Profile)</c>).
        ///   </item>
        ///   <item>
        ///     If the user or their profile is not found, it logs the attempt and returns the appropriate response.
        ///   </item>
        ///   <item>
        ///     It updates the <c>HasHeartProblems</c> value in the profile with the value provided in the model and saves the changes to the database.
        ///   </item>
        ///   <item>
        ///     It logs the successful operation and returns an <c>Ok</c> response with a success message.
        ///   </item>
        /// </list>
        /// </remarks>
        [HttpPost("api/changeHasHeartProblems")]
        public async Task<IActionResult> ChangeHasHeartProblems([FromBody] ChangeHasHeartProblemsViewModel model)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    // Registrar log de erro (dados inválidos)
                    await LogAction("Has heart problems change Attempt", "Failed - Invalid Data", 0);
                    return BadRequest(new { message = "Invalid data" }); // Retorna erro 400
                }

                var userProfile = await _context.Users
                .Include(u => u.Profile)
                   .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (userProfile == null)
                {
                    await LogAction("Has heart problems change Attempt", "Failed - No user found", 0);
                    return Unauthorized(new { message = "No user found" });
                }

                if (userProfile.Profile == null)
                {
                    await LogAction("Has heart problems Attempt", "Failed - Profile not found", userProfile.Id);
                    return NotFound(new { message = "Profile not found" });
                }

                userProfile.Profile.HasHeartProblems = model.HasHeartProblems;
                await _context.SaveChangesAsync();

                await LogAction("Has Heart Problems change Attempt", "Success - Has Heart Problems changed successfully.", userProfile.Id);
                return Ok(new { message = "Has heart problems changed successfully!!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error changing Has heart problems", error = ex.Message });
            }
        }

        /// <summary>
        /// Changes the username registered in a user's profile.
        /// </summary>
        /// <param name="model">
        /// An object of type <see cref="ChangeUsernameViewModel"/> that contains the user's email and the desired new username.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> that indicates:
        /// <list type="bullet">
        ///   <item>
        ///     an <c>Ok</c> response with a success message if the username is changed successfully;
        ///   </item>
        ///   <item>
        ///     a <c>BadRequest</c> response if the provided data is invalid or an error occurs during processing;
        ///   </item>
        ///   <item>
        ///     an <c>Unauthorized</c> response if no user is found with the provided email;
        ///   </item>
        ///   <item>
        ///     a <c>NotFound</c> response if the user's profile is not found or if the new username already exists.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method performs the following operations:
        /// <list type="bullet">
        ///   <item>
        ///     It validates the provided data. If the data is invalid, it logs the attempt and returns a <c>BadRequest</c>.
        ///   </item>
        ///   <item>
        ///     It searches the database for a user by the provided email, including their associated profile (using <c>Include(u => u.Profile)</c>).
        ///   </item>
        ///   <item>
        ///     If the user or their profile is not found, it logs the attempt and returns the appropriate response.
        ///   </item>
        ///   <item>
        ///     It checks if the new username already exists in the database. If it does, it logs the attempt and returns a <c>NotFound</c> response with the message "Username Already Exists".
        ///   </item>
        ///   <item>
        ///     If the username is unique, it updates the <c>Username</c> field in the profile with the new value, saves the changes, and logs the successful operation.
        ///   </item>
        /// </list>
        /// </remarks>
        [HttpPost("api/changeUsername")]
        public async Task<IActionResult> ChangeUsername([FromBody] ChangeUsernameViewModel model)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    // Registrar log de erro (dados inválidos)
                    await LogAction("Username change Attempt", "Failed - Invalid Data", 0);
                    return BadRequest(new { message = "Invalid data" }); // Retorna erro 400
                }

                var userProfile = await _context.Users
                .Include(u => u.Profile)
                   .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (userProfile == null)
                {
                    await LogAction("Username change Attempt", "Failed - No user found", 0);
                    return Unauthorized(new { message = "No user found" });
                }

                if (userProfile.Profile == null)
                {
                    await LogAction("Username change Attempt", "Failed - Profile not found", userProfile.Id);
                    return NotFound(new { message = "Profile not found" });
                }

                var isUnique = await _context.Profiles.FirstOrDefaultAsync(u => u.Username == model.Username);

                if(isUnique != null)
                {
                    await LogAction("Username change Attempt", "Failed - Username Already Exists", userProfile.Id);
                    return NotFound(new { message = "Username Already Exists" });
                }

                userProfile.Profile.Username = model.Username;
                await _context.SaveChangesAsync();

                await LogAction("Username change Attempt", "Success - Username changed successfully.", userProfile.Id);
                return Ok(new { message = "Username changed successfully!!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error changing Username", error = ex.Message });
            }
        }

        /// <summary>
        /// Changes a user's password after validating the old password and sending a notification email.
        /// </summary>
        /// <param name="model">
        /// A <see cref="ChangePasswordViewModel"/> object that contains the user's email, old password, and new password.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> that indicates:
        /// <list type="bullet">
        ///   <item>
        ///     an <c>Ok</c> response with a success message if the password is changed successfully;
        ///   </item>
        ///   <item>
        ///     a <c>BadRequest</c> response if the provided data is invalid or if an error occurs during processing;
        ///   </item>
        ///   <item>
        ///     an <c>Unauthorized</c> response if the user is not found with the provided credentials or if the new password does not meet the required criteria.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method performs the following operations:
        /// <list type="bullet">
        ///   <item>
        ///     It validates the incoming model. If the model is invalid, it logs the attempt and returns a <c>BadRequest</c>.
        ///   </item>
        ///   <item>
        ///     It hashes the old password and searches for a user in the database whose email and hashed old password match the provided values.
        ///   </item>
        ///   <item>
        ///     If the user is not found, it logs the attempt and returns an <c>Unauthorized</c> response.
        ///   </item>
        ///   <item>
        ///     It checks if the new password meets the required criteria. If not, it logs the attempt and returns an <c>Unauthorized</c> response.
        ///   </item>
        ///   <item>
        ///     It sends a notification email to the user indicating that the password has been changed. If the email fails to send, it logs the attempt and returns a server error.
        ///   </item>
        ///   <item>
        ///     It generates a hash for the new password and ensures that it is different from the hash of the old password. If they are the same, it logs the attempt and returns an <c>Unauthorized</c> response.
        ///   </item>
        ///   <item>
        ///     It updates the user's password with the new hash, records the time of the password change, invalidates the current session token,
        ///     saves the changes to the database, and logs the operation as successful.
        ///   </item>
        /// </list>
        /// </remarks>
        [HttpPost("api/changePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordViewModel model)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    // Registrar log de erro (dados inválidos)
                    await LogAction("Password change Attempt", "Failed - Invalid Data", 0);
                    return BadRequest(new { message = "Invalid data" }); // Retorna erro 400
                }

                var hashedOldPassword = HashPassword(model.OldPassword);
                var user = await _context.Users
                   .FirstOrDefaultAsync(u => u.Email == model.Email && u.Password == hashedOldPassword);

                if (user == null)
                {
                    await LogAction("Password change Attempt", "Failed - Old Password is incorrect.", 0);
                    return Unauthorized(new { message = "Old Password is incorrect." });
                }

                if (!IsPasswordValid(model.NewPassword))
                {
                    await LogAction("Password change Attempt", "Failed - Password is not valid", user.Id);
                    return Unauthorized(new { message = "Password is not valid" });
                }

                var passwordChangeNotification = await
                 SendEmail(
                  user.Email,
                  "VitalEase - Email Password Change",
                  "The Password has been updated successfully."
                );

                if (!passwordChangeNotification)
                {
                    await LogAction("Password change Attempt", "Failed - Failed to send password change notification", user.Id);
                    return StatusCode(500, new { message = "Failed to send password change notification to email." });
                }

                var newHashedPassword = HashPassword(model.NewPassword);

                if(hashedOldPassword == newHashedPassword)
                {
                    await LogAction("Password change Attempt", "Failed - Password can not be the same as old one.", user.Id);
                    return Unauthorized(new { message = "Password can not be the same as old one." });
                }


                user.Password = newHashedPassword;
                await _context.SaveChangesAsync();

               
                await LogAction("Password change Attempt", "Success - Password changed successfully.", user.Id);
                return Ok(new { message = "Password changed successfully!!" });
            }
            catch (Exception ex)
            {
                await LogAction("Password change Attempt", "Failed - Error changing Password.", 0);
                return BadRequest(new { message = "Error changing Password", error = ex.Message });
            }
        }

        /// <summary>
        /// Processes a user's email change request.
        /// </summary>
        /// <param name="model">
        /// An object of type <see cref="ChangeEmailViewModel"/> that contains the user's current email, the new email, and the user's password for verification.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> that indicates:
        /// <list type="bullet">
        ///   <item>
        ///     <c>Ok</c> with a success message if the request is processed and the verification emails are successfully sent.
        ///   </item>
        ///   <item>
        ///     <c>BadRequest</c> if the provided data is invalid or an error occurs during processing.
        ///   </item>
        ///   <item>
        ///     <c>Unauthorized</c> if the user is not found, the password is incorrect, or the new email already exists.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method performs the following operations:
        /// <list type="bullet">
        ///   <item>
        ///     Validates the incoming data; if the data is invalid, it logs the change attempt and returns a <c>BadRequest</c>.
        ///   </item>
        ///   <item>
        ///     Hashes the provided password and searches the database for a user whose current email and password match the provided values.
        ///   </item>
        ///   <item>
        ///     If the user is not found, it logs the attempt and returns an <c>Unauthorized</c> response.
        ///   </item>
        ///   <item>
        ///     Checks if the new email already exists in the database; if it does, it logs the attempt and returns an <c>Unauthorized</c> response with the message "Email already exists".
        ///   </item>
        ///   <item>
        ///     Generates a token for verifying the email change, using the current email, the new email, and the user's ID.
        ///   </item>
        ///   <item>
        ///     Creates two verification links: one for the new email and another for the current (old) email, ensuring the token is URL-escaped.
        ///   </item>
        ///   <item>
        ///     Sends the verification links via email to both the old and new email addresses. If sending fails for either address,
        ///     it logs the attempt and returns a server error.
        ///   </item>
        ///   <item>
        ///     Logs the operation as successful and returns an <c>Ok</c> response with a success message.
        ///   </item>
        /// </list>
        /// </remarks>
        [HttpPost("api/changeEmail")]
        public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailViewModel model)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    // Registrar log de erro (dados inválidos)
                    await LogAction("Email change Attempt", "Failed - Invalid Data", 0);
                    return BadRequest(new { message = "Invalid data" }); // Retorna erro 400
                }

                var hashedPassword = HashPassword(model.Password);

                var user = await _context.Users
                   .FirstOrDefaultAsync(u => u.Email == model.Email && u.Password == hashedPassword);


                if (user == null)
                {
                    await LogAction("Email change Attempt", "Failed - No user found", 0);
                    return Unauthorized(new { message = "No user found" });
                }

                var emailAlreadyExists = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.NewEmail);

                if (emailAlreadyExists != null)
                {
                    await LogAction("Email change Attempt", "Failed - Email already exists", user.Id);
                    return Unauthorized(new { message = "Email already exists" });
                }

                var token = GenerateToken(user.Email, model.NewEmail, user.Id);

                var changeEmailLink = $"https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/confirmNewEmail?token={Uri.EscapeDataString(token)}";

                var emailSent = await SendEmailResetLink(model.NewEmail, changeEmailLink);
                if (!emailSent)
                {
                    await LogAction("Email change Attempt", "Failed - Failed to send change email to new email", user.Id);
                    return StatusCode(500, new { message = "Failed to send change email to new email." });
                }

                var changeOldEmailLink = $"https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/confirmOldEmail?token={Uri.EscapeDataString(token)}";

                var emailSentOldEmail = await SendEmailResetLink(user.Email, changeOldEmailLink);
                if (!emailSentOldEmail)
                {
                    await LogAction("Email change Attempt", "Failed - Failed to send change email to old email", user.Id);
                    return StatusCode(500, new { message = "Failed to send change email to old email." });
                }


                await LogAction("Email change Attempt", "Success - Change emails are sent to old and new email's.", user.Id);
                return Ok(new { message = "Email verification has been sent to your old email and new email"});
            }
            catch (Exception ex)
            {
                await LogAction("Email change Attempt", "Failed - Error changing Email.", 0);
                return BadRequest(new { message = "Error changing Email", error = ex.Message });
            }
        }

        /// <summary>
        /// Validates the token for confirming the new email and, if valid, confirms the change.
        /// </summary>
        /// <param name="model">
        /// An object of type <see cref="ConfirmNewEmailViewModel"/> that contains the token provided in the query string.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> that indicates:
        /// <list type="bullet">
        ///   <item>
        ///     an <c>Ok</c> response with a success message and the new email if the token is valid,
        ///   </item>
        ///   <item>
        ///     a <c>BadRequest</c> response if the token is expired or invalid, logging the error and returning an appropriate message.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method performs the following operations:
        /// <list type="bullet">
        ///   <item>
        ///     It validates the token using the <c>ValidateToken</c> method, which returns a tuple containing the validity status, the old email, the new email, the user ID, and the tokenId.
        ///   </item>
        ///   <item>
        ///     If the token is not valid, it logs the change attempt and returns a <c>BadRequest</c> with the message "Token expired."
        ///   </item>
        ///   <item>
        ///     If the token is valid, it saves any pending changes to the database and returns an <c>Ok</c> response with a success message and the new email.
        ///   </item>
        /// </list>
        /// </remarks>
        [HttpGet("api/ValidateNewEmailToken")]
        public async Task<IActionResult>ConfirmNewEmailToken([FromQuery] ConfirmNewEmailViewModel model)
        {
            var (isValid, oldEmail, newEmail, userId, tokenId) = ValidateToken(model.Token);

            if (!isValid)
            {
                await LogAction("Email change Attempt", "Failed - Error changing Email.", 0);
                return BadRequest(new { message = "Token expired." });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Token is valid.", newEmail });
        }

        /// <summary>
        /// Confirms the email change request by validating the provided token, updating the user's email, and marking the reset token as used.
        /// </summary>
        /// <param name="model">
        /// A <see cref="ConfirmNewEmailViewModel"/> containing the token sent in the query string.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> that indicates:
        /// <list type="bullet">
        ///   <item>
        ///     <c>Ok</c> with a success message and the new email if the token is valid and the email change is confirmed.
        ///   </item>
        ///   <item>
        ///     <c>BadRequest</c> if the token is invalid/expired, if the user is not found, if the reset token is already used, or if the email has already been verified.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// The method performs the following steps:
        /// <list type="bullet">
        ///   <item>
        ///     Validates the token using the <c>ValidateToken</c> method, which returns a tuple containing the validity status, the old email, the new email, the user ID, and the token ID.
        ///   </item>
        ///   <item>
        ///     If the token is invalid, it logs the attempt and returns a <c>BadRequest</c> with the message "Token expired."
        ///   </item>
        ///   <item>
        ///     Retrieves the user based on the user ID extracted from the token. If the user is not found, it logs the attempt and returns a <c>BadRequest</c>.
        ///   </item>
        ///   <item>
        ///     Checks the reset email token in the database using the token ID to ensure it has not been used yet. If it has been used, it logs the attempt and returns a <c>BadRequest</c> with the message "Email already verified."
        ///   </item>
        ///   <item>
        ///     If the reset token is valid and marked as used on the old email, the method updates the user's email to the new email, logs the successful operation, sends a success notification email, and returns an <c>Ok</c> response.
        ///   </item>
        ///   <item>
        ///     If the conditions for the reset token are not fully met, it still saves any pending changes and returns an <c>Ok</c> with a message indicating that the token is valid along with the new email.
        ///   </item>
        /// </list>
        /// </remarks>
        [HttpGet("api/ConfirmNewEmailChange")]
        public async Task<IActionResult> ConfirmNewEmailChange([FromQuery] ConfirmNewEmailViewModel model)
        {
            var (isValid, oldEmail, newEmail, userId, tokenId) = ValidateToken(model.Token);

            if (!isValid)
            {
                await LogAction("Email change Attempt", "Failed - Error changing Email.", 0);
                return BadRequest(new { message = "Token expired." });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);


            if (user == null)
            {
                await LogAction("Email change Attempt", "Failed - User not found.", 0);
                return BadRequest(new { message = "User not found" });
            }

            var resetEmailToken = await _context.ResetEmailTokens.FirstOrDefaultAsync(u => u.TokenId == tokenId);

            if (resetEmailToken != null && resetEmailToken.IsUsed != true)
            {

                resetEmailToken.IsUsed = true;
            }
            else
            {
                await LogAction("Email change Attempt", "Failed - Email already verified.", userId);
                return BadRequest(new { message = "Email already verified." });
            }

            if (resetEmailToken.IsUsed == true && resetEmailToken.IsUsedOnOldEmail == true)
            {
                await LogAction("Email change Attempt", "Success - Email changed successfully.", userId);
                user.Email = newEmail;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                var successNotification = await
                SendEmail(
                    newEmail,
                    "VitalEase - Email Change Success",
                    "The email change has been completed successfully."
                );

                if (!successNotification)
                {
                    return StatusCode(500, new { message = "Failed to send cancel notification email to old email." });
                }

                return Ok(new { message = "Email changed successfully.", newEmail });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Token is valid.", newEmail });
        }

        /// <summary>
        /// Cancels the new email change request by removing the associated reset email token 
        /// and sending a cancellation notification to the user's old email address.
        /// </summary>
        /// <param name="model">
        /// A <see cref="ConfirmNewEmailViewModel"/> containing the token used for validating 
        /// the email change request.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> that indicates:
        /// <list type="bullet">
        ///   <item>
        ///     <c>Ok</c> with a success message and the new email if the cancellation is successful.
        ///   </item>
        ///   <item>
        ///     <c>BadRequest</c> if the token is expired or if there is an error sending the cancellation notification.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// The method performs the following operations:
        /// <list type="bullet">
        ///   <item>
        ///     Validates the token using the <c>ValidateToken</c> method, which returns a tuple containing the validity,
        ///     the old email, the new email, the user ID, and the token ID.
        ///   </item>
        ///   <item>
        ///     If the token is not valid, logs the attempt and returns a <c>BadRequest</c> with the message "Token expired."
        ///   </item>
        ///   <item>
        ///     Retrieves the reset email token from the database using the token ID, and if found, removes it.
        ///   </item>
        ///   <item>
        ///     Sends a cancellation notification email to the old email address.
        ///   </item>
        ///   <item>
        ///     Logs the cancellation of the email change attempt, saves changes to the database,
        ///     and returns an <c>Ok</c> response with a success message and the new email.
        ///   </item>
        /// </list>
        /// </remarks>
        [HttpGet("api/CancelNewEmailChange")]
        public async Task<IActionResult> CancelNewEmailChange([FromQuery] ConfirmNewEmailViewModel model)
        {
            var (isValid, oldEmail, newEmail, userId, tokenId) = ValidateToken(model.Token);

            if (!isValid)
            {
                await LogAction("Email change Attempt", "Failed - Token Expired.", 0);
                return BadRequest(new { message = "Token expired." });
            }

            var resetEmailToken = await _context.ResetEmailTokens.FirstOrDefaultAsync(u => u.TokenId == tokenId);

            if (resetEmailToken != null)
            {
                 _context.ResetEmailTokens.Remove(resetEmailToken);
            }


            var cancelNotification = await 
                SendEmail(
                    oldEmail,
                    "VitalEase - Email Change Cancelation",
                    "The email change has been canceled."
                );
            
            if (!cancelNotification)
            {
                return StatusCode(500, new { message = "Failed to send cancel notification email to old email." });
            }

            await LogAction("Email change Attempt", "Change email successfully canceled.", userId);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Change email successfully canceled.", newEmail });
        }

        /// <summary>
        /// Validates the token for confirming the old email change and returns a success message if the token is valid.
        /// </summary>
        /// <param name="model">
        /// A <see cref="ConfirmOldEmailViewModel"/> containing the token used for validating the old email change request.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> that indicates:
        /// <list type="bullet">
        ///   <item>
        ///     <c>Ok</c> with a success message and the new email if the token is valid.
        ///   </item>
        ///   <item>
        ///     <c>BadRequest</c> with the message "Token expired." if the token is invalid or expired.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method performs the following operations:
        /// <list type="bullet">
        ///   <item>
        ///     It validates the provided token using the <c>ValidateTokenOnOldEmail</c> method, which returns a tuple containing the validity status,
        ///     the old email, the new email, the user ID, and the token ID.
        ///   </item>
        ///   <item>
        ///     If the token is not valid, the method logs the attempt and returns a <c>BadRequest</c> with the message "Token expired."
        ///   </item>
        ///   <item>
        ///     If the token is valid, any pending changes are saved to the database and an <c>Ok</c> response is returned with a success message and the new email.
        ///   </item>
        /// </list>
        /// </remarks>
        [HttpGet("api/ValidateOldEmailToken")]
        public async Task<IActionResult> ConfirmOldEmailToken([FromQuery] ConfirmOldEmailViewModel model)
        {
            var (isValid, oldEmail, newEmail,userId, tokenId) = ValidateTokenOnOldEmail(model.Token);

            if (!isValid)
            {
                await LogAction("Email change Attempt", "Failed - Token Expired.", 0);
                return BadRequest(new { message = "Token expired." });
            }

          
            await _context.SaveChangesAsync();
            return Ok(new { message = "Token is valid.", newEmail });
        }

        /// <summary>
        /// Confirms the change of the old email as part of the email change process.
        /// </summary>
        /// <param name="model">
        /// A <see cref="ConfirmOldEmailViewModel"/> containing the token used to validate the old email change request.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> that indicates:
        /// <list type="bullet">
        ///   <item>
        ///     <c>Ok</c> with a success message and the new email if the token is valid, the user is found, and the reset token has been properly marked as used.
        ///   </item>
        ///   <item>
        ///     <c>BadRequest</c> if the token is expired, invalid, the user is not found, or if the reset token indicates that the email has already been verified.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This endpoint performs the following steps:
        /// <list type="bullet">
        ///   <item>
        ///     Validates the provided token using the <c>ValidateTokenOnOldEmail</c> method, which returns a tuple containing the validity status, old email, new email, user ID, and token ID.
        ///   </item>
        ///   <item>
        ///     If the token is not valid, logs the attempt and returns a <c>BadRequest</c> with the message "Token expired."
        ///   </item>
        ///   <item>
        ///     Retrieves the user by the user ID extracted from the token. If no user is found, logs the attempt and returns a <c>BadRequest</c> with the message "User not found."
        ///   </item>
        ///   <item>
        ///     Checks the reset email token associated with the token ID. If the token is not marked as used on the old email, marks it as used; otherwise, logs the attempt and returns a <c>BadRequest</c> with the message "Email already verified."
        ///   </item>
        ///   <item>
        ///     If the reset email token is marked as used (both overall and on the old email), updates the user's email to the new email, logs the successful email change, sends a success notification email to the new email, and returns an <c>Ok</c> response.
        ///   </item>
        ///   <item>
        ///     If the above conditions are not fully met, it saves any pending changes and returns an <c>Ok</c> response indicating that the token is valid along with the new email.
        ///   </item>
        /// </list>
        /// </remarks>
        [HttpGet("api/ConfirmOldEmailToken")]
        public async Task<IActionResult> ConfirmOldEmailChange([FromQuery] ConfirmOldEmailViewModel model)
        {
            var (isValid, oldEmail, newEmail, userId, tokenId) = ValidateTokenOnOldEmail(model.Token);

            if (!isValid)
            {
                await LogAction("Email change Attempt", "Failed - Token Expired.", 0);
                return BadRequest(new { message = "Token expired." });
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                await LogAction("Email change Attempt", "Failed - User not found.", 0);
                return BadRequest(new { message = "User not found." });
            }

            var resetEmailToken = _context.ResetEmailTokens.FirstOrDefault(u => u.TokenId == tokenId);

            if (resetEmailToken != null && resetEmailToken.IsUsedOnOldEmail != true)
            {

                resetEmailToken.IsUsedOnOldEmail = true;
            }
            else
            {
                await LogAction("Email change Attempt", "Failed - Email already verified.", userId);
                return BadRequest(new { message = "Email already verified." });
            }

            if (resetEmailToken.IsUsed == true && resetEmailToken.IsUsedOnOldEmail == true)
            {
                await LogAction("Email change Attempt", "Success - Email changed successfully.", userId);
                user.Email = newEmail;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                var successNotification = await
                SendEmail(
                    newEmail,
                    "VitalEase - Email Change Success",
                    "The email change has been completed successfully."
                );

                if (!successNotification)
                {
                    return StatusCode(500, new { message = "Failed to send cancel notification email to old email." });
                }

                return Ok(new { message = "Email changed successfully.", newEmail });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Token is valid.", newEmail });
        }

        /// <summary>
        /// Cancels the old email change process by removing the associated reset email token 
        /// and sending a cancellation notification to the user's old email address.
        /// </summary>
        /// <param name="model">
        /// A <see cref="ConfirmOldEmailViewModel"/> containing the token used for validating the old email change request.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> that indicates:
        /// <list type="bullet">
        ///   <item>
        ///     <c>Ok</c> with a success message and the new email if the cancellation is completed successfully.
        ///   </item>
        ///   <item>
        ///     <c>BadRequest</c> if the token is expired or if an error occurs during the process.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This endpoint performs the following operations:
        /// <list type="bullet">
        ///   <item>
        ///     Validates the provided token using the <c>ValidateToken</c> method, which returns a tuple containing 
        ///     the validity status, the old email, the new email, the user ID, and the token ID.
        ///   </item>
        ///   <item>
        ///     If the token is invalid, logs the attempt and returns a <c>BadRequest</c> with the message "Token expired."
        ///   </item>
        ///   <item>
        ///     Retrieves the reset email token from the database using the token ID and removes it if found.
        ///   </item>
        ///   <item>
        ///     Sends a cancellation notification email to the old email address.
        ///   </item>
        ///   <item>
        ///     Logs the cancellation attempt, saves any pending changes to the database, 
        ///     and returns an <c>Ok</c> response with a success message and the new email.
        ///   </item>
        /// </list>
        /// </remarks>
        [HttpGet("api/CancelOldEmailChange")]
        public async Task<IActionResult> CancelOldEmailChange([FromQuery] ConfirmOldEmailViewModel model)
        {
            var (isValid, oldEmail, newEmail,userId, tokenId) = ValidateToken(model.Token);

            if (!isValid)
            {
                await LogAction("Email change Attempt", "Failed - Token Expired.", 0);
                return BadRequest(new { message = "Token expired." });
            }

            var resetEmailToken = await _context.ResetEmailTokens.FirstOrDefaultAsync(u => u.TokenId == tokenId);

            if (resetEmailToken != null)
            {
                _context.ResetEmailTokens.Remove(resetEmailToken);
            }

            var cancelNotification = await
                SendEmail(
                    oldEmail,
                    "VitalEase - Email Change Cancelation",
                    "The email change has been canceled."
                );

            if (!cancelNotification)
            {
                return StatusCode(500, new { message = "Failed to send cancel notification email to old email." });
            }

            await LogAction("Email change Attempt", "Change email successfully canceled.", userId);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Change email successfully canceled.", newEmail });
        }


        /// <summary>
        /// Validates the token for confirming the new email and, if valid, confirms the change.
        /// </summary>
        /// <param name="model">
        /// An object of type <see cref="ConfirmNewEmailViewModel"/> that contains the token provided in the query string.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> that indicates:
        /// <list type="bullet">
        ///   <item>
        ///     an <c>Ok</c> response with a success message and the new email if the token is valid,
        ///   </item>
        ///   <item>
        ///     a <c>BadRequest</c> response if the token is expired or invalid, logging the error and returning an appropriate message.
        ///   </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method performs the following operations:
        /// <list type="bullet">
        ///   <item>
        ///     It validates the token using the <c>ValidateToken</c> method, which returns a tuple containing the validity,
        ///     the old email, the new email, the user ID, and the tokenId.
        ///   </item>
        ///   <item>
        ///     If the token is not valid, it logs the attempt and returns a <c>BadRequest</c> with the message "Token expired."
        ///   </item>
        ///   <item>
        ///     If the token is valid, it saves any pending changes to the database and returns an <c>Ok</c> response with a success message and the new email.
        ///   </item>
        /// </list>
        /// </remarks>
        [HttpGet("api/ValidateDeleteAccountToken")]
        public async Task<IActionResult> ConfirmDeleteAccountToken([FromQuery] DeleteAccountTokenViewModel model)
        {
            var (isValid, email, userId, tokenId) = ValidateDeleteAccountToken(model.Token);

            if (!isValid)
            {
                await LogAction("Delete Account Attempt", "Failed - Token expired.", userId);
                return BadRequest(new { message = "Token expired." });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Token is valid.", email });
        }

        /// <summary>
        /// Sends an email with the specified subject and body to the given email address.
        /// </summary>
        /// <param name="toEmail">The recipient's email address.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="body">The HTML-formatted body of the email.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a boolean value:
        /// <c>true</c> if the email was sent successfully; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method retrieves email settings from the configuration, including the sender's email, SMTP server details, and credentials.
        /// It first validates that the sender's email is properly configured and in a valid format, as well as ensuring that the SMTP port is a valid number.
        /// It then constructs a <see cref="MailMessage"/> with the provided subject and body, and attempts to send it using an <see cref="SmtpClient"/> configured for SSL.
        /// Any exceptions encountered during the sending process are caught, logged to the console for debugging purposes, and the method returns <c>false</c>.
        /// </remarks>
        private async Task<bool> SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                var fromEmail = _configuration["EmailSettings:FromEmail"];
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPortString = _configuration["EmailSettings:SmtpPort"];
                var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
                var smtpPassword = _configuration["EmailSettings:SmtpPassword"];

                // Verificar se o fromEmail está configurado corretamente
                if (string.IsNullOrEmpty(fromEmail) || !IsValidEmail(fromEmail))
                {
                    throw new Exception("From email is not valid or not configured.");
                }

                // Verificar se o smtpPort é um número válido
                if (!int.TryParse(smtpPortString, out var smtpPort))
                {
                    throw new Exception("SMTP port is not a valid number.");
                }

                var message = new MailMessage
                {
                    From = new MailAddress(fromEmail), // Definindo o endereço de 'from' corretamente
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                message.To.Add(toEmail); // Adicionando o destinatário do e-mail

                using (var client = new SmtpClient(smtpServer, smtpPort))
                {
                    client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                    client.EnableSsl = true; // Garantir que o envio seja seguro via SSL
                    await client.SendMailAsync(message);
                }

                return true;
            }
            catch (Exception ex)
            {
                // Para fins de depuração, vamos logar a mensagem de erro
                Console.WriteLine($"Error occurred while sending email: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Sends an email containing a reset link to confirm an email change request.
        /// </summary>
        /// <param name="toEmail">The recipient's email address.</param>
        /// <param name="emailLink">The URL that the recipient must click to confirm the email change.</param>
        /// <returns>
        /// A task that represents the asynchronous operation, returning <c>true</c> if the email was sent successfully,
        /// or <c>false</c> if an error occurred.
        /// </returns>
        /// <remarks>
        /// This method retrieves the necessary email configuration settings (e.g., sender's email, SMTP server details, port, and credentials)
        /// from the configuration. It first validates that the sender's email is properly configured and is in a valid format,
        /// and that the SMTP port is a valid number. It then constructs an HTML email with the subject "Change email request"
        /// and an HTML anchor tag embedding the provided <paramref name="emailLink"/>. The email is sent using an SMTP client
        /// configured to use SSL for secure transmission. Any exceptions encountered during the email sending process are logged,
        /// and the method returns <c>false</c>.
        /// </remarks>
        private async Task<bool> SendEmailResetLink(string toEmail, string emailLink)
        {
            try
            {
                var fromEmail = _configuration["EmailSettings:FromEmail"];
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPortString = _configuration["EmailSettings:SmtpPort"];
                var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
                var smtpPassword = _configuration["EmailSettings:SmtpPassword"];

                // Verificar se o fromEmail está configurado corretamente
                if (string.IsNullOrEmpty(fromEmail) || !IsValidEmail(fromEmail))
                {
                    throw new Exception("From email is not valid or not configured.");
                }

                // Verificar se o smtpPort é um número válido
                if (!int.TryParse(smtpPortString, out var smtpPort))
                {
                    throw new Exception("SMTP port is not a valid number.");
                }

                var message = new MailMessage
                {
                    From = new MailAddress(fromEmail), // Definindo o endereço de 'from' corretamente
                    Subject = "Change email request",
                    Body = $"Click the following link to confirm your email change: <a href='{emailLink}'>Change email link</a>",
                    IsBodyHtml = true
                };

                message.To.Add(toEmail); // Adicionando o destinatário do e-mail

                using (var client = new SmtpClient(smtpServer, smtpPort))
                {
                    client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                    client.EnableSsl = true; // Garantir que o envio seja seguro via SSL
                    await client.SendMailAsync(message);
                }

                return true;
            }
            catch (Exception ex)
            {
                // Para fins de depuração, vamos logar a mensagem de erro
                Console.WriteLine($"Error occurred while sending email: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Sends an email containing a delete account link to confirm an delete account request.
        /// </summary>
        /// <param name="toEmail">The recipient's email address.</param>
        /// <param name="emailLink">The URL that the recipient must click to confirm the email change.</param>
        /// <returns>
        /// A task that represents the asynchronous operation, returning <c>true</c> if the email was sent successfully,
        /// or <c>false</c> if an error occurred.
        /// </returns>
        /// <remarks>
        /// This method retrieves the necessary email configuration settings (e.g., sender's email, SMTP server details, port, and credentials)
        /// from the configuration. It first validates that the sender's email is properly configured and is in a valid format,
        /// and that the SMTP port is a valid number. It then constructs an HTML email with the subject "Delete Accouont request"
        /// and an HTML anchor tag embedding the provided <paramref name="emailLink"/>. The email is sent using an SMTP client
        /// configured to use SSL for secure transmission. Any exceptions encountered during the email sending process are logged,
        /// and the method returns <c>false</c>.
        /// </remarks>
        private async Task<bool> SendDeleteAccountLink(string toEmail, string emailLink)
        {
            try
            {
                var fromEmail = _configuration["EmailSettings:FromEmail"];
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPortString = _configuration["EmailSettings:SmtpPort"];
                var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
                var smtpPassword = _configuration["EmailSettings:SmtpPassword"];

                // Verificar se o fromEmail está configurado corretamente
                if (string.IsNullOrEmpty(fromEmail) || !IsValidEmail(fromEmail))
                {
                    throw new Exception("From email is not valid or not configured.");
                }

                // Verificar se o smtpPort é um número válido
                if (!int.TryParse(smtpPortString, out var smtpPort))
                {
                    throw new Exception("SMTP port is not a valid number.");
                }

                var message = new MailMessage
                {
                    From = new MailAddress(fromEmail), // Definindo o endereço de 'from' corretamente
                    Subject = "Delete Account request",
                    Body = $"Click the following link to confirm your account deletion: <a href='{emailLink}'>Delete account request link</a>",
                    IsBodyHtml = true
                };

                message.To.Add(toEmail); // Adicionando o destinatário do e-mail

                using (var client = new SmtpClient(smtpServer, smtpPort))
                {
                    client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                    client.EnableSsl = true; // Garantir que o envio seja seguro via SSL
                    await client.SendMailAsync(message);
                }

                return true;
            }
            catch (Exception ex)
            {
                // Para fins de depuração, vamos logar a mensagem de erro
                Console.WriteLine($"Error occurred while sending email: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Determines whether the specified email address is valid.
        /// </summary>
        /// <param name="email">The email address to validate.</param>
        /// <returns>
        /// <c>true</c> if the email address is valid; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method attempts to create a new instance of <see cref="System.Net.Mail.MailAddress"/> using the provided email.
        /// If the instantiation succeeds and the constructed address matches the input, the email is considered valid.
        /// If an exception occurs, the method catches it and returns <c>false</c>.
        /// </remarks>
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Logs an audit action by recording it in the database.
        /// </summary>
        /// <param name="action">The action that was performed.</param>
        /// <param name="status">The status or outcome of the action.</param>
        /// <param name="UserId">The identifier of the user associated with the action.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation of logging the action.
        /// </returns>
        /// <remarks>
        /// This method creates an <see cref="AuditLog"/> object with the current time, the specified action, status, and user ID,
        /// adds it to the database context, and saves the changes asynchronously.
        /// </remarks>
        private async Task LogAction(string action, string status, int UserId)
        {
            var log = new AuditLog
            {
                Timestamp = DateTime.Now,
                Action = action,
                Status = status,
                UserId = UserId
            };

            // Salvar o log no banco de dados
            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Validates a password based on predefined complexity requirements.
        /// </summary>
        /// <param name="password">The password to validate.</param>
        /// <returns>
        /// <c>true</c> if the password meets all the requirements; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// The password is considered valid if it meets all of the following criteria:
        /// <list type="bullet">
        ///   <item>It must be at least 12 characters long.</item>
        ///   <item>It must contain at least one lowercase letter.</item>
        ///   <item>It must contain at least one uppercase letter.</item>
        ///   <item>It must contain at least one special character from the set: !@#$%^&*(),.?":{}|<> (including a space).</item>
        /// </list>
        /// </remarks>
        private bool IsPasswordValid(string password)
        {
            // Verificar se a senha tem pelo menos 12 caracteres
            if (password.Length < 12)
            {
                return false;
            }

            // Verificar se a senha contém pelo menos uma letra minúscula
            if (!password.Any(char.IsLower))
            {
                return false;
            }

            // Verificar se a senha contém pelo menos uma letra maiúscula
            if (!password.Any(char.IsUpper))
            {
                return false;
            }

            // Verificar se a senha contém pelo menos um caractere especial
            var specialChars = "!@#$%^&*(),.?\":{}|<> ";
            if (!password.Any(c => specialChars.Contains(c)))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Computes the SHA256 hash of a given password and returns it as a hexadecimal string.
        /// </summary>
        /// <param name="password">The plain text password to hash.</param>
        /// <returns>
        /// A hexadecimal string representation of the SHA256 hash of the provided password.
        /// </returns>
        /// <remarks>
        /// This method converts the input password into a byte array using UTF8 encoding, computes its SHA256 hash,
        /// and then constructs a hexadecimal string from the resulting byte array.
        /// </remarks>
        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Converte a senha para um array de bytes e gera o hash
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Converte o hash para uma string hexadecimal
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// Generates a JWT token for changing the email address, incorporating the old email, the new email, the user ID, and a unique token identifier.
        /// </summary>
        /// <param name="oldEmail">The user's current email address.</param>
        /// <param name="newEmail">The new email address to be set for the user.</param>
        /// <param name="userId">The identifier of the user.</param>
        /// <returns>
        /// A string representing the generated JWT token, valid for 60 minutes.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the JWT key is not properly configured in the application.
        /// </exception>
        /// <remarks>
        /// This method performs the following operations:
        /// <list type="bullet">
        ///   <item>
        ///     Retrieves the JWT key from the application settings and verifies its existence.
        ///   </item>
        ///   <item>
        ///     Generates a unique token identifier (tokenId).
        ///   </item>
        ///   <item>
        ///     Creates a JWT token with the following claims:
        ///     <list type="bullet">
        ///       <item>The old email.</item>
        ///       <item>The new email.</item>
        ///       <item>The user's identifier.</item>
        ///       <item>The tokenId.</item>
        ///     </list>
        ///   </item>
        ///   <item>
        ///     Sets the token's expiration to 60 minutes from creation, using a symmetric key and the HMAC SHA256 algorithm for signing.
        ///   </item>
        ///   <item>
        ///     Creates a record in <see cref="ResetEmailTokens"/> with the tokenId, creation date, expiration date, and usage flags initially set to <c>false</c>.
        ///   </item>
        ///   <item>
        ///     Adds the token record to the database, saves the changes, and returns the generated JWT token.
        ///   </item>
        /// </list>
        /// </remarks>
        public string GenerateToken(string oldEmail, string newEmail, int userId)
        {

            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new ArgumentNullException("Jwt:Key", "A chave JWT não está configurada corretamente.");
            }

            var tokenId = Guid.NewGuid().ToString();

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiresAt = DateTime.Now.AddMinutes(60);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: new[]
                {
                    new Claim(ClaimTypes.Email, oldEmail),
                    new Claim(ClaimTypes.Email, newEmail),
                    new Claim("userId", userId.ToString()),
                    new Claim("tokenId", tokenId)
                },
                expires: expiresAt, // Define o tempo de expiração do token (1 hora, por exemplo)
                signingCredentials: creds
            );

            var resetEmailToken = new ResetEmailTokens
            {
                TokenId = tokenId,         // Usando o tokenId gerado
                CreatedAt = DateTime.Now,
                ExpiresAt = expiresAt,
                IsUsed = false,
                IsUsedOnOldEmail = false,
            };

            _context.ResetEmailTokens.Add(resetEmailToken);
            _context.SaveChanges();

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Validates the JWT token.
        /// </summary>
        /// <param name="token">The token to be validated.</param>
        /// <returns>
        /// A tuple containing: whether the token is valid, the email, and the userId.
        /// </returns>
        private (bool IsValid, string oldEmail, string newEmail, int UserId, string idToken) ValidateToken(string token)
        {
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new ArgumentNullException("Jwt:Key", "A chave JWT não está configurada corretamente.");
            }
            var key = Encoding.UTF8.GetBytes(jwtKey);
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                // Tente validar o token com os parâmetros necessários.
                var claimsPrincipal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true, // Validar a chave de assinatura
                    IssuerSigningKey = new SymmetricSecurityKey(key), // Usar a chave para validação
                    ValidateIssuer = true, // Validar o emissor
                    ValidateAudience = true, // Validar o público
                    ValidIssuer = _configuration["Jwt:Issuer"], // Emissor configurado
                    ValidAudience = _configuration["Jwt:Audience"], // Público configurado
                    ClockSkew = TimeSpan.FromMinutes(5) // Permitir uma margem de 5 minutos para a expiração
                }, out _);

                // Obter o email e userId do token
                var oldEmail = claimsPrincipal.FindAll(ClaimTypes.Email)?.ToList()[0].Value;
                var newEmail = claimsPrincipal.FindAll(ClaimTypes.Email)?.ToList()[1].Value;
                var userIdClaim = claimsPrincipal.FindFirst("userId");
                var userId = userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
                var tokenIdClaim = claimsPrincipal.FindFirst("tokenId");
                var tokenId = tokenIdClaim != null ? tokenIdClaim.Value : "";
                
                // Validar se o email e userId são válidos
                if (string.IsNullOrEmpty(oldEmail) || string.IsNullOrEmpty(newEmail) || userId == 0)
                {
                    return (false, "", "", 0,tokenId);
                }
                var tokenRecord = _context.ResetEmailTokens
              .FirstOrDefault(l => l.TokenId == tokenId);

                if (tokenRecord == null)
                {
                    // Token não encontrado na base de dados
                    return (false, oldEmail, newEmail, userId,tokenId);
                }

                if (tokenRecord.IsUsed)
                {
                    // Se o token já foi usado, retorna falso
                    return (false, oldEmail, newEmail, userId,tokenId);
                }

                // Se chegou até aqui, o token é válido
                return (true, oldEmail, newEmail, userId,tokenId);
            }
            catch (SecurityTokenExpiredException)
            {
                // Token expirado
                return (false, "", "", 0,"");
            }
            catch (SecurityTokenException)
            {
                // Token inválido, mas não expirado
                return (false, "", "", 0, "");
            }
            catch (Exception ex)
            {
                // Outros erros ao validar
                Console.WriteLine($"Error during token validation: {ex.Message}");
                return (false, "", "", 0, "");
            }
        }

        /// <summary>
        /// Generates a JWT token for deleting an account and records the token details in the database.
        /// </summary>
        /// <param name="email">The email address associated with the account to be deleted.</param>
        /// <param name="userId">The identifier of the user requesting account deletion.</param>
        /// <returns>
        /// A string representing the generated JWT token.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the JWT key is not properly configured.
        /// </exception>
        /// <remarks>
        /// This method performs the following steps:
        /// <list type="bullet">
        ///   <item>
        ///     Retrieves the JWT secret key from the configuration and ensures it is not empty. If empty, an exception is thrown.
        ///   </item>
        ///   <item>
        ///     Generates a unique token identifier (tokenId) using <c>Guid.NewGuid()</c>.
        ///   </item>
        ///   <item>
        ///     Creates a symmetric security key using the secret key and defines signing credentials with the HMAC SHA256 algorithm.
        ///   </item>
        ///   <item>
        ///     Generates a JWT token with claims for the user's email, userId, and tokenId, and sets its expiration to 24 hours from the current time.
        ///   </item>
        ///   <item>
        ///     Creates a new <see cref="DeleteAccountTokens"/> record with the generated token details (tokenId, creation time, expiration time, and usage status) and adds it to the database.
        ///   </item>
        ///   <item>
        ///     Saves the changes to the database and returns the JWT token as a string.
        ///   </item>
        /// </list>
        /// </remarks>
        public string GenerateDeleteAccountToken(string email, int userId)
        {

            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new ArgumentNullException("Jwt:Key", "A chave JWT não está configurada corretamente.");
            }

            var tokenId = Guid.NewGuid().ToString();

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiresAt = DateTime.Now.AddHours(24);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: new[]
                {
                    new Claim(ClaimTypes.Email, email),
                    new Claim("userId", userId.ToString()),
                    new Claim("tokenId", tokenId)
                },
                expires: expiresAt, // Define o tempo de expiração do token (1 hora, por exemplo)
                signingCredentials: creds
            );

            var deleteAccountToken = new DeleteAccountTokens
            {
                TokenId = tokenId,         // Usando o tokenId gerado
                CreatedAt = DateTime.Now,
                ExpiresAt = expiresAt,
                IsUsed = false,
            };

            _context.DeleteAccountTokens.Add(deleteAccountToken);
            _context.SaveChanges();


            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Validates the JWT token used for deleting an account and extracts related information.
        /// </summary>
        /// <param name="token">The JWT token to validate.</param>
        /// <returns>
        /// A tuple containing:
        /// <list type="bullet">
        ///   <item><c>true</c> if the token is valid; otherwise, <c>false</c>.</item>
        ///   <item>The email extracted from the token.</item>
        ///   <item>The user ID extracted from the token.</item>
        ///   <item>The token identifier (tokenId) extracted from the token.</item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method performs the following operations:
        /// <list type="bullet">
        ///   <item>
        ///     Retrieves the JWT secret key from the configuration and validates that it is properly set. 
        ///     If the key is missing or empty, an <see cref="ArgumentNullException"/> is thrown.
        ///   </item>
        ///   <item>
        ///     Creates a symmetric security key from the secret and validates the provided token using the specified token validation parameters,
        ///     including issuer, audience, and a clock skew of 5 minutes.
        ///   </item>
        ///   <item>
        ///     Extracts the email, user ID, and token ID from the token's claims.
        ///   </item>
        ///   <item>
        ///     Checks if the extracted email is empty or if the user ID is 0, and returns <c>false</c> if so.
        ///   </item>
        ///   <item>
        ///     Queries the database to find the corresponding delete account token record using the token ID.
        ///   </item>
        ///   <item>
        ///     Returns <c>false</c> if the token record is not found or if it has already been used.
        ///   </item>
        ///   <item>
        ///     Returns <c>true</c> along with the extracted email, user ID, and token ID if the token is valid and has not been used.
        ///   </item>
        ///   <item>
        ///     Catches and handles specific exceptions such as <see cref="SecurityTokenExpiredException"/> and <see cref="SecurityTokenException"/>,
        ///     returning <c>false</c> if the token is expired or invalid, and logging any other exceptions.
        ///   </item>
        /// </list>
        /// </remarks>
        private (bool IsValid, string email, int UserId, string idToken) ValidateDeleteAccountToken(string token)
        {
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new ArgumentNullException("Jwt:Key", "A chave JWT não está configurada corretamente.");
            }
            var key = Encoding.UTF8.GetBytes(jwtKey);
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                // Tente validar o token com os parâmetros necessários.
                var claimsPrincipal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true, // Validar a chave de assinatura
                    IssuerSigningKey = new SymmetricSecurityKey(key), // Usar a chave para validação
                    ValidateIssuer = true, // Validar o emissor
                    ValidateAudience = true, // Validar o público
                    ValidIssuer = _configuration["Jwt:Issuer"], // Emissor configurado
                    ValidAudience = _configuration["Jwt:Audience"], // Público configurado
                    ClockSkew = TimeSpan.FromMinutes(5) // Permitir uma margem de 5 minutos para a expiração
                }, out _);

                // Obter o email e userId do token
                var email = claimsPrincipal.FindAll(ClaimTypes.Email)?.ToList()[0].Value;
                var userIdClaim = claimsPrincipal.FindFirst("userId");
                var userId = userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
                var tokenIdClaim = claimsPrincipal.FindFirst("tokenId");
                var tokenId = tokenIdClaim != null ? tokenIdClaim.Value : "";

                if (string.IsNullOrEmpty(email) || userId == 0)
                {
                    return (false, "", 0, tokenId);
                }

                var tokenRecord = _context.DeleteAccountTokens
              .FirstOrDefault(l => l.TokenId == tokenId);

                if (tokenRecord == null)
                {
                    // Token não encontrado na base de dados
                    return (false, email, userId, tokenId);
                }

                if (tokenRecord.IsUsed)
                {
                    // Se o token já foi usado, retorna falso
                    return (false, email, userId, tokenId);
                }

                // Se chegou até aqui, o token é válido
                return (true,email, userId, tokenId);
            }
            catch (SecurityTokenExpiredException)
            {
                // Token expirado
                return (false, "", 0, "");
            }
            catch (SecurityTokenException)
            {
                // Token inválido, mas não expirado
                return (false, "", 0, "");
            }
            catch (Exception ex)
            {
                // Outros erros ao validar
                Console.WriteLine($"Error during token validation: {ex.Message}");
                return (false, "", 0, "");
            }
        }

        /// <summary>
        /// Validates the JWT token.
        /// </summary>
        /// <param name="token">The token to be validated.</param>
        /// <returns>
        /// A tuple containing:
        /// <list type="bullet">
        ///   <item>
        ///     a boolean indicating whether the token is valid,
        ///   </item>
        ///   <item>
        ///     the email associated with the token,
        ///   </item>
        ///   <item>
        ///     and the userId extracted from the token.
        ///   </item>
        /// </list>
        /// </returns>
        private (bool IsValid, string oldEmail, string newEmail ,int UserId, string idToken) ValidateTokenOnOldEmail(string token)
        {
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new ArgumentNullException("Jwt:Key", "A chave JWT não está configurada corretamente.");
            }
            var key = Encoding.UTF8.GetBytes(jwtKey);
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                // Tente validar o token com os parâmetros necessários.
                var claimsPrincipal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true, // Validar a chave de assinatura
                    IssuerSigningKey = new SymmetricSecurityKey(key), // Usar a chave para validação
                    ValidateIssuer = true, // Validar o emissor
                    ValidateAudience = true, // Validar o público
                    ValidIssuer = _configuration["Jwt:Issuer"], // Emissor configurado
                    ValidAudience = _configuration["Jwt:Audience"], // Público configurado
                    ClockSkew = TimeSpan.FromMinutes(5) // Permitir uma margem de 5 minutos para a expiração
                }, out _);

                // Obter o email e userId do token
                var oldEmail = claimsPrincipal.FindAll(ClaimTypes.Email)?.ToList()[0].Value;
                var newEmail = claimsPrincipal.FindAll(ClaimTypes.Email)?.ToList()[1].Value;
                var userIdClaim = claimsPrincipal.FindFirst("userId");
                var userId = userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
                var tokenIdClaim = claimsPrincipal.FindFirst("tokenId");
                var tokenId = tokenIdClaim != null ? tokenIdClaim.Value : "";

                // Validar se o email e userId são válidos
                if (string.IsNullOrEmpty(oldEmail) || string.IsNullOrEmpty(newEmail) || userId == 0)
                {
                    return (false, "", "", 0, tokenId);
                }
                var tokenRecord = _context.ResetEmailTokens
              .FirstOrDefault(l => l.TokenId == tokenId);

                if (tokenRecord == null)
                {
                    // Token não encontrado na base de dados
                    return (false, oldEmail, newEmail, userId, tokenId);
                }

                if (tokenRecord.IsUsedOnOldEmail)
                {
                    // Se o token já foi usado, retorna falso
                    return (false, oldEmail, newEmail, userId, tokenId);
                }

                // Se chegou até aqui, o token é válido
                return (true, oldEmail, newEmail, userId, tokenId);
            }
            catch (SecurityTokenExpiredException)
            {
                // Token expirado
                return (false, "", "", 0, "");
            }
            catch (SecurityTokenException)
            {
                // Token inválido, mas não expirado
                return (false, "", "", 0, "");
            }
            catch (Exception ex)
            {
                // Outros erros ao validar
                Console.WriteLine($"Error during token validation: {ex.Message}");
                return (false, "", "", 0, "");
            }
        }
    }
}