﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Logic.Services;
using Logic.Models;
using System.Collections.Generic;

namespace StoreClient
{
    public partial class MainWindow : Window
    {
        private double windowWidth = 1280, windowHeight = 720;
        private List<Product> productsForSale = new List<Product>();
        private ProductService productService;
        private CartService cartService;
        private CheckoutService checkoutService;
        private ListBox productsBox, cartBox;
        private TextBlock cartTotalPrice;
        private TextBox couponTextBox;
        private Button cartBtn, backToMainBtn, checkoutBtn, applyCouponBtn;
        private Border mainBorder;
        private StackPanel currentPanel, mainPanel, cartPanel;

        public MainWindow()
        {
            InitializeComponent();
            this.productService = new ProductService { };
            cartService = new CartService { };
            checkoutService = new CheckoutService { };
            Start();
        }

        private void FetchProducts()
        {
            try
            {
                this.productsForSale = this.productService.FetchProducts();
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

            productsBox = new ListBox
            {
                FontSize = 24,
                BorderBrush = Brushes.Red,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(30, -150, 0, -300),
                Padding = new Thickness(5)
            };
            productsBox.MouseDoubleClick += ProductDoubleClick;
            SetupProductList();

            cartBtn = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(50),
                Padding = new Thickness(20),
                Content = "Show Cart"
            };
            cartBtn.Click += CartBtnClick;

            p.Width = windowWidth;
            p.Height = windowHeight;
            p.Children.Add(storeText);
            p.Children.Add(productListText);
            p.Children.Add(cartBtn);
            p.Children.Add(productsBox);

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
                Margin = new Thickness(10),
                FontSize = 30,
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = "Cart"
            };
            TextBlock productListText = new TextBlock
            {
                Margin = new Thickness(40, 0, 0, 0),
                FontSize = 20,
                HorizontalAlignment = HorizontalAlignment.Left,
                Text = "Currencies added to cart"
            };
            cartTotalPrice = new TextBlock
            {
                Margin = new Thickness(0, 0, 0, 0),
                Padding = new Thickness(5),
                FontSize = 30,
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = "Total price: $0"
            };

            cartBox = new ListBox
            {
                FontSize = 24,
                Width = 250,
                BorderBrush = Brushes.Red,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(30, -150, 0, -300),
                Padding = new Thickness(5)
            };

            backToMainBtn = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                Width = 75,
                Margin = new Thickness(50),
                Padding = new Thickness(20),
                Content = "Back"
            };
            backToMainBtn.Click += BackToMainBtnClick;

            checkoutBtn = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                Width = 75,
                Margin = new Thickness(400, 145, 50, 0),
                Padding = new Thickness(20),
                Content = "Checkout"
            };
            checkoutBtn.Click += CheckoutBtnClick;

            applyCouponBtn = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 100,
                Margin = new Thickness(0, 0, 0, 50),
                Padding = new Thickness(20),
                Content = "Apply Coupon"
            };
            applyCouponBtn.Click += ApplyCouponBtnClick;

            couponTextBox = new TextBox
            {
                FontSize = 14,
                Width = 100,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 0),
                Padding = new Thickness(5)
            };

            p.Width = windowWidth;
            p.Height = windowHeight;
            p.Children.Add(storeText);
            p.Children.Add(productListText);
            p.Children.Add(backToMainBtn);
            p.Children.Add(cartBox);
            p.Children.Add(checkoutBtn);
            p.Children.Add(couponTextBox);
            p.Children.Add(cartTotalPrice);

            return p;
        }

        private void SetupProductList()
        {
            foreach (Product p in productsForSale)
            {
                productsBox.Items.Add($"{p.title} - ${p.price}");
            }
        }

        private void CartBtnClick(object sender, RoutedEventArgs e)
        {
            if (cartService.GetCart().Count > 0)
            {
                foreach (Cart c in cartService.GetCart())
                {
                    cartBox.Items.Add($"{c.productName} - ${c.price}");
                }
            }
            else
            {
                MessageBox.Show("No items added to cart!", "Alert");
            }

            double totalCartPrice = checkoutService.Checkout(cartService.GetCart()).totalPrice;
            cartTotalPrice.Text = $"Total price: ${totalCartPrice}";
            SetLayout(cartPanel);
        }

        private void BackToMainBtnClick(object sender, RoutedEventArgs e)
        {
            SetLayout(mainPanel);
            cartBox.Items.Clear();
        }

        private void ApplyCouponBtnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("TODO ADD COUPON");
        }

        private void ProductDoubleClick(object sender, MouseEventArgs e)
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
            Receipt receipt = checkoutService.Checkout(cartService.GetCart());

            string msg = "";
            foreach (Cart c in receipt.products)
            {
                msg += $"{c.productName} - {c.price}";
            }

            MessageBox.Show(msg);
            MessageBox.Show("Total price: " + receipt.totalPrice);
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
