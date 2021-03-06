﻿namespace MyCoolWebServer.ByTheCakeApplication.Controllers
{
    using Infrastructure;
    using Services.Contracts;
    using Server.Http;
    using Server.Http.Contracts;
    using Server.Http.Response;
    using ViewModels;
    using ViewModels.Account;
    using Services;
    using System;

    public class AccountController : Controller
    {
        private const string RegisterView = "account\\register";
        private const string LoginView = "account\\login";

        private readonly IUserService userService;

        public AccountController()
        {
            this.userService = new UserService();
        }

        // HttpGet
        public IHttpResponse Register()
        {
            this.ViewData["showError"] = "none";
            this.ViewData["showLogout"] = "none";
            return this.FileViewResponse(RegisterView);
        }

        // HttpPost
        public IHttpResponse Register(IHttpRequest request, RegisterUserViewModel model)
        {
            if (model.Username.Length < 3
                || model.Password.Length < 3
                || model.ConfirmPassword.Length != model.Password.Length)
            {
                this.AddError("Invalid user credentials.");

                return this.FileViewResponse(RegisterView);
            }

            var success = this.userService.Create(model.Username, model.Password);

            if (success)
            {
                this.LoginUser(request, model.Username);

                return new RedirectResponse("/");
            }
            else
            {
                this.AddError("Username is already taken.");

                return this.FileViewResponse(RegisterView);
            }
        }

        public IHttpResponse Login()
        {
            this.ViewData["showLogout"] = "none";
            this.ViewData["showError"] = "none";

            return this.FileViewResponse("account\\login");
        }

        public IHttpResponse Login(IHttpRequest request, LoginUserViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Username)
                || string.IsNullOrWhiteSpace(model.Password))
            {
                this.AddError("All credentials are compulsory.");

                return this.FileViewResponse(LoginView);
            }

            var exists = this.userService.Exists(model.Username, model.Password);

            if (exists)
            {
                this.LoginUser(request, model.Username);

                return new RedirectResponse("/");
            }
            else
            {
                this.AddError("The credentials do not match.");

                return this.FileViewResponse(LoginView);
            }
        }

        public IHttpResponse Profile(IHttpRequest request)
        {
            if (!request.Session.Contains(SessionStore.CurrentUserKey))
            {
                throw new InvalidOperationException("There is no logged in user.");
            }

            var username = request.Session.Get<string>(SessionStore.CurrentUserKey);
            var profile = userService.Get(username);

            if (profile == null)
            {
                throw new InvalidOperationException($"The user {username} could not be found in the database.");
            }

            this.ViewData["name"] = profile.Username;
            this.ViewData["registerDate"] = profile.RegistrationDate.ToShortDateString();
            this.ViewData["ordersCount"] = profile.TotalOrders.ToString();

            return this.FileViewResponse("home\\profile");
        }

        public IHttpResponse Logout(IHttpRequest request)
        {
            request.Session.Clear();

            return this.FileViewResponse("account\\logout");
        }

        private void LoginUser(IHttpRequest request, string username)
        {
            request.Session.Add(SessionStore.CurrentUserKey, username);
            request.Session.Add(ShoppingCart.SessionKey, new ShoppingCart());
        }
    }
}
