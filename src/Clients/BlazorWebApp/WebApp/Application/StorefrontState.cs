using Microsoft.AspNetCore.Components.Authorization;
using WebApp.Application.Services.Interfaces;
using WebApp.Domain.Models;
using WebApp.Infrastructure.Auth;

namespace WebApp.Application
{
    public record ToastMessage(Guid Id, string Text, bool IsError);

    //per circuit state shared by header, cards and pages
    public class StorefrontState : IDisposable
    {
        private readonly IBasketService _basketService;
        private readonly IWishlistService _wishlistService;
        private readonly ICatalogService _catalogService;
        private readonly CustomAuthStateProvider _authStateProvider;
        private readonly ILogger<StorefrontState> _logger;

        private Task _initTask;

        public StorefrontState(IBasketService basketService, IWishlistService wishlistService, ICatalogService catalogService,
            CustomAuthStateProvider authStateProvider, ILogger<StorefrontState> logger)
        {
            _basketService = basketService;
            _wishlistService = wishlistService;
            _catalogService = catalogService;
            _authStateProvider = authStateProvider;
            _logger = logger;

            _authStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;
        }

        public event Action Changed;

        public int CartCount { get; private set; }

        public HashSet<int> WishlistIds { get; private set; } = new();

        public List<Category> Categories { get; private set; } = new();

        public List<ToastMessage> Toasts { get; } = new();

        public bool IsAuthenticated { get; private set; }

        public UserSession Session => _authStateProvider.Session;

        public bool AnnouncementClosed { get; set; }

        //first caller loads, everybody else awaits the same task
        public Task EnsureLoadedAsync() => _initTask ??= LoadAsync();

        private async Task LoadAsync()
        {
            try
            {
                Categories = await _catalogService.GetCategoriesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Categories could not be loaded");
            }

            await LoadUserDataAsync();
        }

        private async Task LoadUserDataAsync()
        {
            var state = await _authStateProvider.GetAuthenticationStateAsync();
            IsAuthenticated = state.User.Identity?.IsAuthenticated == true;

            if (!IsAuthenticated)
            {
                CartCount = 0;
                WishlistIds = new HashSet<int>();
                Changed?.Invoke();
                return;
            }

            try
            {
                var basketTask = _basketService.GetBasketAsync();
                var wishlistTask = _wishlistService.GetIdsAsync();

                CartCount = (await basketTask).Count;
                WishlistIds = (await wishlistTask).ToHashSet();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Basket or wishlist could not be loaded");
            }

            Changed?.Invoke();
        }

        public async Task RefreshCartAsync()
        {
            try
            {
                CartCount = IsAuthenticated ? (await _basketService.GetBasketAsync()).Count : 0;
                Changed?.Invoke();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Basket count could not be refreshed");
            }
        }

        public void SetCartCount(int count)
        {
            CartCount = count;
            Changed?.Invoke();
        }

        //returns false when the user has to log in first
        public async Task<bool> ToggleWishlistAsync(int productId)
        {
            if (!IsAuthenticated)
                return false;

            if (WishlistIds.Contains(productId))
            {
                await _wishlistService.RemoveAsync(productId);
                WishlistIds.Remove(productId);
            }
            else
            {
                await _wishlistService.AddAsync(productId);
                WishlistIds.Add(productId);
            }

            Changed?.Invoke();
            return true;
        }

        public async Task<bool> AddToCartAsync(int productId, int? offerId, int quantity = 1)
        {
            if (!IsAuthenticated)
                return false;

            await _basketService.AddItemAsync(productId, offerId, quantity);
            await RefreshCartAsync();
            return true;
        }

        public void ShowToast(string text, bool isError = false)
        {
            var toast = new ToastMessage(Guid.NewGuid(), text, isError);
            Toasts.Add(toast);
            Changed?.Invoke();

            //toast disappears by itself
            _ = Task.Delay(3500).ContinueWith(_ =>
            {
                Toasts.Remove(toast);
                Changed?.Invoke();
            });
        }

        private void OnAuthenticationStateChanged(Task<AuthenticationState> task)
        {
            _ = LoadUserDataAsync();
        }

        public void Dispose()
        {
            _authStateProvider.AuthenticationStateChanged -= OnAuthenticationStateChanged;
        }
    }
}
