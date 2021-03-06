﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Logic.Services;
using Logic.Models;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace StoreClient
{
    public partial class MainWindow : Window
    {
        private double windowWidth = 1280, windowHeight = 720;
        private List<Product> productsForSale = new List<Product>();
        private ProductService productService;
        private CartService cartService;
        private CheckoutService checkoutService;
        private CouponService couponService;
        private Coupon appliedCoupon;
        private Image currencyImage;
        private ListView productsBox, cartBox;
        private TextBlock cartTotalPrice, currencyCostText, currencyDescriptionText, receiptText;
        private TextBox couponTextBox;
        private Button cartBtn, backToMainBtn, checkoutBtn, applyCouponBtn, addToCartBtn, loadCartBtn, SaveCartBtn, clearCartBtn;
        private Border mainBorder;
        private StackPanel currentPanel, mainPanel, receiptPanel, cartPanel;
        private double totalCartPrice;

        public MainWindow()
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            InitializeComponent();
            productService = new ProductService { };
            cartService = new CartService { };
            checkoutService = new CheckoutService { };
            couponService = new CouponService { };
            Start();
        }

        private void FetchProducts()
        {
            try
            {
                productsForSale = productService.FetchProducts();
            }
            catch (IndexOutOfRangeException e)
            {
                MessageBox.Show($"Error: {e.Message}");
            }
        }

        private void Start()
        {
            // Window settings
            Title = "Store";
            Width = windowWidth;
            Height = windowHeight;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            SizeChanged += OnWindowSizeChanged;

            // Element setup
            mainBorder = new Border();
            mainBorder.Background = Brushes.LightBlue;
            mainBorder.BorderBrush = Brushes.Black;
            mainBorder.Padding = new Thickness(15);
            mainBorder.BorderThickness = new Thickness(1);

            FetchProducts();
            mainPanel = MainLayout();
            cartPanel = CartLayout();
            receiptPanel = ReceiptLayout();

            SetLayout(mainPanel);
        }

        private void SetLayout(StackPanel panel)
        {
            currentPanel = panel;
            mainBorder.Child = panel;
            Content = mainBorder;
            ResizeCurrentPanel();
        }

        private StackPanel MainLayout()
        {
            StackPanel p = new StackPanel()
            {
                Background = Brushes.Azure,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top
            };

            TextBlock storeText = new TextBlock
            {
                Margin = new Thickness(10),
                FontSize = 30,
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = "Cryptocurrency Store"
            };

            TextBlock productListText = new TextBlock
            {
                Margin = new Thickness(40, 0, 0, 0),
                FontSize = 20,
                HorizontalAlignment = HorizontalAlignment.Left,
                Text = "Available cryptocurrencies"
            };

            productsBox = new ListView
            {
                FontSize = 24,
                Width = 250,
                BorderBrush = Brushes.Red,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(30, 50, 0, 0),
                Padding = new Thickness(5)
            };
            productsBox.SelectionChanged += ProductSelectionChanged;

            SetupProductList();

            cartBtn = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                Width = 125,
                Height = 75,
                Margin = new Thickness(0, -97, 25, 0),
                Padding = new Thickness(5),
                Content = "Show Cart"
            };
            cartBtn.Click += CartBtnClick;

            addToCartBtn = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 75,
                Height = 25,
                Margin = new Thickness(0, 0, 0, 0),
                Padding = new Thickness(5),
                Visibility = Visibility.Hidden,
                Content = "Add to cart"
            };
            addToCartBtn.Click += AddToCartBtnClick;

            currencyImage = new Image
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                MaxWidth = 256,
                MaxHeight = 256,
                Width = 256,
                Height = 256,
                Margin = new Thickness(0, -250, 0, 0),
                Visibility = Visibility.Hidden,
            };
            currencyCostText = new TextBlock
            {
                Margin = new Thickness(0, -275, 0, 0),
                FontSize = 20,
                HorizontalAlignment = HorizontalAlignment.Center,
                Visibility = Visibility.Hidden,
                Text = "$100"
            };

            currencyDescriptionText = new TextBlock
            {
                Margin = new Thickness(0, 25, 0, 0),
                MaxWidth = 500,
                TextAlignment = TextAlignment.Center,
                FontSize = 20,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Visibility = Visibility.Hidden,
                Text = "A description"
            };

            p.Width = windowWidth;
            p.Height = windowHeight;
            p.Children.Add(storeText);
            p.Children.Add(productListText);
            p.Children.Add(cartBtn);
            p.Children.Add(productsBox);
            p.Children.Add(currencyCostText);
            p.Children.Add(currencyImage);
            p.Children.Add(addToCartBtn);
            p.Children.Add(currencyDescriptionText);

            return p;
        }
        private StackPanel CartLayout()
        {
            StackPanel p = new StackPanel()
            {
                Background = Brushes.Beige,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top
            };
            TextBlock storeText = new TextBlock
            {
                Margin = new Thickness(0, -50, 0, 0),
                FontSize = 30,
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = "Cart"
            };
            TextBlock productListText = new TextBlock
            {
                Margin = new Thickness(40, -550, 0, 550),
                FontSize = 20,
                HorizontalAlignment = HorizontalAlignment.Left,
                Text = "Currencies added to cart"
            };
            cartTotalPrice = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, -450, 0, 450),
                Padding = new Thickness(1),
                FontSize = 42,
                Text = "Total price: $0"
            };

            cartBox = new ListView
            {
                FontSize = 24,
                Width = 250,
                Height = 450,
                BorderBrush = Brushes.Red,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(30, 0, 0, 0),
                Padding = new Thickness(1)
            };

            ContextMenu contextMenu = new ContextMenu();
            contextMenu.Items.Add("Remove");
            contextMenu.AddHandler(MenuItem.ClickEvent, new RoutedEventHandler(RemoveCartItem));
            cartBox.ContextMenu = contextMenu;

            backToMainBtn = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                Width = 125,
                Height = 50,
                Margin = new Thickness(0, 0, 25, 0),
                Padding = new Thickness(5),
                Content = "Back"
            };
            backToMainBtn.Click += BackToMainBtnClick;

            SaveCartBtn = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = 150,
                Height = 40,
                Margin = new Thickness(220, -90, 0, 50),
                Padding = new Thickness(1),
                Content = "Save cart"
            };
            SaveCartBtn.Click += SaveCartBtnClick;

            loadCartBtn = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = 150,
                Height = 40,
                Margin = new Thickness(60, -60, 0, 50),
                Padding = new Thickness(1),
                Content = "Load cart"
            };
            loadCartBtn.Click += LoadCartBtnClick;

            clearCartBtn = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = 150,
                Height = 40,
                Margin = new Thickness(60, -40, 0, 25),
                Padding = new Thickness(1),
                Content = "Clear cart"
            };
            clearCartBtn.Click += ClearCartBtnClick;

            checkoutBtn = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                Width = 125,
                Height = 50,
                Margin = new Thickness(0, -135, 25, 115),
                Padding = new Thickness(1),
                Content = "Checkout"
            };
            checkoutBtn.Click += CheckoutBtnClick;

            applyCouponBtn = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 125,
                Height = 50,
                Margin = new Thickness(0, -100, 0, 100),
                Padding = new Thickness(1),
                Content = "Apply Coupon"
            };
            applyCouponBtn.Click += ApplyCouponBtnClick;

            couponTextBox = new TextBox
            {
                FontSize = 14,
                Width = 150,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, -20, 0, 20),
                Padding = new Thickness(1)
            };

            p.Width = windowWidth;
            p.Height = windowHeight;
            p.Children.Add(backToMainBtn);
            p.Children.Add(storeText);
            p.Children.Add(cartBox);
            p.Children.Add(cartTotalPrice);
            p.Children.Add(productListText);
            p.Children.Add(loadCartBtn);
            p.Children.Add(SaveCartBtn);
            p.Children.Add(clearCartBtn);
            p.Children.Add(couponTextBox);
            p.Children.Add(applyCouponBtn);
            p.Children.Add(checkoutBtn);

            return p;
        }
        private StackPanel ReceiptLayout()
        {
            StackPanel p = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Background = Brushes.DarkKhaki
            };

            receiptText = new TextBlock()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 1000,
                Height = 600,
                FontSize = 30,
                Foreground = Brushes.White
            };

            p.Children.Add(receiptText);
            return p;
        }
        private void SetupProductList()
        {
            foreach (Product p in productsForSale)
            {
                productsBox.Items.Add($"{p.title}");
            }
        }

        private void UpdatePrice()
        {
            totalCartPrice = 0;
            cartService.GetCart().ForEach(product => totalCartPrice += product.price);

            if (appliedCoupon != null)
            {
                totalCartPrice = totalCartPrice - (totalCartPrice * appliedCoupon.discount);
            }
            cartTotalPrice.Text = $"Total price: ${totalCartPrice}";
        }

        private void RemoveCartItem(object sender, RoutedEventArgs e)
        {
            cartService.RemoveItemFromCart(cartBox.SelectedIndex);
            cartBox.Items.Remove(cartBox.SelectedItem);
            UpdatePrice();
        }

        private void ProductSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (currencyImage.Visibility == Visibility.Hidden)
            {
                currencyImage.Visibility = Visibility.Visible;
                currencyCostText.Visibility = Visibility.Visible;
                currencyDescriptionText.Visibility = Visibility.Visible;
                addToCartBtn.Visibility = Visibility.Visible;
            }

            try
            {
                currencyImage.Source = new BitmapImage(new Uri(Logic.IO.FileHelper.GetProjectFilePath(productsForSale[productsBox.SelectedIndex].imagePath)));
                currencyCostText.Text = $"${productsForSale[productsBox.SelectedIndex].price}";
                currencyDescriptionText.Text = $"{productsForSale[productsBox.SelectedIndex].description}";
            }
            catch
            {
                MessageBox.Show("Failed to load image, no image found!");
            }
        }

        private void CartBtnClick(object sender, RoutedEventArgs e)
        {
            if (cartService.GetCart().Count > 0)
            {
                cartService.GetCart().ForEach(p =>
                {
                    cartBox.Items.Add($"{p.title} - ${p.price}");
                });

                UpdatePrice();
            }

            SetLayout(cartPanel);
        }

        private void BackToMainBtnClick(object sender, RoutedEventArgs e)
        {
            SetLayout(mainPanel);
            cartBox.Items.Clear();
            checkoutService.ClearReceipt();
        }

        private void SaveCartBtnClick(object sender, RoutedEventArgs e)
        {
            if (cartBox.Items.Count > 0)
            {
                cartService.SaveCart();
                MessageBox.Show("Saved current cart.", "Cart");
                return;
            }

            MessageBox.Show("No items in cart!");
        }

        private void LoadCartBtnClick(object sender, RoutedEventArgs e)
        {
            cartBox.Items.Clear();
            cartService.LoadCart().ForEach(p => cartBox.Items.Add($"{p.title} - ${p.price}"));
            UpdatePrice();
            MessageBox.Show("Loaded saved cart", "Cart");
        }

        private void ClearCartBtnClick(object sender, RoutedEventArgs e)
        {
            cartService.ClearCart();
            cartBox.Items.Clear();
            UpdatePrice();
            MessageBox.Show("Cart cleared!");
        }

        private void ApplyCouponBtnClick(object sender, RoutedEventArgs e)
        {
            Coupon coupon = couponService.ValidateCoupon(couponTextBox.Text);
            if (coupon == null)
            {
                MessageBox.Show("Coupon not found!");
                return;
            }

            if (coupon == appliedCoupon)
            {
                MessageBox.Show("Coupon already applied!");
                return;
            }

            appliedCoupon = coupon;
            UpdatePrice();
            MessageBox.Show("Coupon applied!");
        }

        private void AddToCartBtnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Product product = productsForSale[productsBox.SelectedIndex];
                MessageBox.Show($"You added 1x {product.title} to cart!", "Cart");
                cartService.AddItemToCart(product);
            }
            catch
            {
                return;
            }
        }

        private void CheckoutBtnClick(object sender, RoutedEventArgs e)
        {
            if (cartBox.Items.Count <= 0)
            {
                MessageBox.Show("You haven't added any items to cart!");
                return;
            }

            Receipt receipt;
            if (appliedCoupon == null)
            {

                receipt = checkoutService.Checkout(cartService.GetCart());
            }
            else
            {
                receipt = checkoutService.Checkout(cartService.GetCart(), appliedCoupon.code);
            }
            string receiptMessage = "";
            foreach (Product p in receipt.products)
            {
                receiptMessage += $"\n{p.title} - ${p.price}".ToUpper();
            }

            receiptMessage += $"\n\nTOTAL DISCOUNT: {receipt.discount}% - ${receipt.totalPrice - totalCartPrice}";
            receiptMessage += $"\nTOTAL PRICE: ${totalCartPrice}";
            receiptText.Text = receiptMessage;

            SetLayout(receiptPanel);
        }

        private void ResizeCurrentPanel()
        {
            currentPanel.Width = windowWidth;
            currentPanel.Height = windowHeight;
        }

        protected void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            windowWidth = e.NewSize.Width;
            windowHeight = e.NewSize.Height;

            if (currentPanel != null)
            {
                ResizeCurrentPanel();
            }
        }
    }
}
