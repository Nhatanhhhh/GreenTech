using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    slug = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    parent_id = table.Column<int>(type: "int", nullable: true),
                    image = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    sort_order = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.id);
                    table.ForeignKey(
                        name: "FK_categories_categories_parent_id",
                        column: x => x.parent_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "coupon_templates",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    discount_type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    discount_value = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    min_order_amount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    points_cost = table.Column<int>(type: "int", nullable: false),
                    usage_limit_per_user = table.Column<int>(type: "int", nullable: false),
                    total_usage_limit = table.Column<int>(type: "int", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    valid_days = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_coupon_templates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    role_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "suppliers",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    contact_person = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    phone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    tax_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    payment_terms = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_suppliers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    full_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    phone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    province = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    district = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ward = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    specific_address = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    avatar = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    points = table.Column<int>(type: "int", nullable: false),
                    wallet_balance = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    email_verified_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    phone_verified_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    sku = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    slug = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    short_description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    category_id = table.Column<int>(type: "int", nullable: false),
                    supplier_id = table.Column<int>(type: "int", nullable: false),
                    cost_price = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    sell_price = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    image = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    care_instructions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    plant_size = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    weight = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    dimensions = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    tags = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    points_earned = table.Column<int>(type: "int", nullable: false),
                    is_featured = table.Column<bool>(type: "bit", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    seo_title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    seo_description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products", x => x.id);
                    table.ForeignKey(
                        name: "FK_products_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_products_suppliers_supplier_id",
                        column: x => x.supplier_id,
                        principalTable: "suppliers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "banners",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    image_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    link_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    position = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sort_order = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    start_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    end_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    click_count = table.Column<int>(type: "int", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_banners", x => x.id);
                    table.ForeignKey(
                        name: "FK_banners_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "blogs",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    slug = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    excerpt = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    featured_image = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    author_id = table.Column<int>(type: "int", nullable: false),
                    category_id = table.Column<int>(type: "int", nullable: true),
                    tags = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    view_count = table.Column<int>(type: "int", nullable: false),
                    is_featured = table.Column<bool>(type: "bit", nullable: false),
                    is_published = table.Column<bool>(type: "bit", nullable: false),
                    published_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    seo_title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    seo_description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blogs", x => x.id);
                    table.ForeignKey(
                        name: "FK_blogs_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_blogs_users_author_id",
                        column: x => x.author_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "coupons",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    template_id = table.Column<int>(type: "int", nullable: true),
                    user_id = table.Column<int>(type: "int", nullable: true),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    discount_type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    discount_value = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    min_order_amount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    usage_limit = table.Column<int>(type: "int", nullable: false),
                    used_count = table.Column<int>(type: "int", nullable: false),
                    source = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    points_used = table.Column<int>(type: "int", nullable: false),
                    start_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    end_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_coupons", x => x.id);
                    table.ForeignKey(
                        name: "FK_coupons_coupon_templates_template_id",
                        column: x => x.template_id,
                        principalTable: "coupon_templates",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_coupons_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    reference_id = table.Column<int>(type: "int", nullable: true),
                    priority = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_read = table.Column<bool>(type: "bit", nullable: false),
                    read_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.id);
                    table.ForeignKey(
                        name: "FK_notifications_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "point_earning_rules",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    points_per_amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    min_order_amount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    max_points_per_order = table.Column<int>(type: "int", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    valid_from = table.Column<DateTime>(type: "datetime2", nullable: false),
                    valid_until = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_by = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_point_earning_rules", x => x.id);
                    table.ForeignKey(
                        name: "FK_point_earning_rules_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false),
                    role_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "FK_user_roles_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_roles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_images",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    product_id = table.Column<int>(type: "int", nullable: false),
                    image_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    alt_text = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    is_primary = table.Column<bool>(type: "bit", nullable: false),
                    sort_order = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_images", x => x.id);
                    table.ForeignKey(
                        name: "FK_product_images_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_rating_stats",
                columns: table => new
                {
                    product_id = table.Column<int>(type: "int", nullable: false),
                    total_reviews = table.Column<int>(type: "int", nullable: false),
                    average_rating = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: false),
                    star_1_count = table.Column<int>(type: "int", nullable: false),
                    star_2_count = table.Column<int>(type: "int", nullable: false),
                    star_3_count = table.Column<int>(type: "int", nullable: false),
                    star_4_count = table.Column<int>(type: "int", nullable: false),
                    star_5_count = table.Column<int>(type: "int", nullable: false),
                    with_media_count = table.Column<int>(type: "int", nullable: false),
                    with_content_count = table.Column<int>(type: "int", nullable: false),
                    last_updated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_rating_stats", x => x.product_id);
                    table.ForeignKey(
                        name: "FK_product_rating_stats_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "carts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    session_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    coupon_id = table.Column<int>(type: "int", nullable: true),
                    total_items = table.Column<int>(type: "int", nullable: false),
                    subtotal = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    discount_amount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_carts", x => x.id);
                    table.ForeignKey(
                        name: "FK_carts_coupons_coupon_id",
                        column: x => x.coupon_id,
                        principalTable: "coupons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_carts_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    order_number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    coupon_id = table.Column<int>(type: "int", nullable: true),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    payment_status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    payment_gateway = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    subtotal = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    discount_amount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    shipping_fee = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    total = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    wallet_amount_used = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    gateway_transaction_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    shipping_address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    customer_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    customer_phone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    points_earned = table.Column<int>(type: "int", nullable: false),
                    points_awarded_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    note = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    cancelled_reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    cancelled_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    shipped_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delivered_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orders", x => x.id);
                    table.ForeignKey(
                        name: "FK_orders_coupons_coupon_id",
                        column: x => x.coupon_id,
                        principalTable: "coupons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_orders_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "point_transactions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    transaction_type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    points = table.Column<int>(type: "int", nullable: false),
                    reference_type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    reference_id = table.Column<int>(type: "int", nullable: true),
                    point_earning_rule_id = table.Column<int>(type: "int", nullable: true),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    points_before = table.Column<int>(type: "int", nullable: false),
                    points_after = table.Column<int>(type: "int", nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_point_transactions", x => x.id);
                    table.ForeignKey(
                        name: "FK_point_transactions_point_earning_rules_point_earning_rule_id",
                        column: x => x.point_earning_rule_id,
                        principalTable: "point_earning_rules",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_point_transactions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cart_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    cart_id = table.Column<int>(type: "int", nullable: false),
                    product_id = table.Column<int>(type: "int", nullable: false),
                    product_sku = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    product_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    product_image = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    unit_price = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    subtotal = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    is_available = table.Column<bool>(type: "bit", nullable: false),
                    added_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cart_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_cart_items_carts_cart_id",
                        column: x => x.cart_id,
                        principalTable: "carts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_cart_items_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    order_id = table.Column<int>(type: "int", nullable: false),
                    product_id = table.Column<int>(type: "int", nullable: false),
                    product_sku = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    product_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    unit_cost_price = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    unit_sell_price = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    total = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    points_per_item = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_order_items_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_order_items_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "wallet_transactions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    transaction_type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    payment_gateway = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    gateway_transaction_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    order_id = table.Column<int>(type: "int", nullable: true),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    balance_before = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    balance_after = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    processed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wallet_transactions", x => x.id);
                    table.ForeignKey(
                        name: "FK_wallet_transactions_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_wallet_transactions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reviews",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    product_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    order_item_id = table.Column<int>(type: "int", nullable: false),
                    rating = table.Column<int>(type: "int", nullable: false),
                    content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    media_urls = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    helpful_count = table.Column<int>(type: "int", nullable: false),
                    is_anonymous = table.Column<bool>(type: "bit", nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reviews", x => x.id);
                    table.ForeignKey(
                        name: "FK_reviews_order_items_order_item_id",
                        column: x => x.order_item_id,
                        principalTable: "order_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reviews_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reviews_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "review_replies",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    review_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_review_replies", x => x.id);
                    table.ForeignKey(
                        name: "FK_review_replies_reviews_review_id",
                        column: x => x.review_id,
                        principalTable: "reviews",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_review_replies_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "review_votes",
                columns: table => new
                {
                    review_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    is_helpful = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_review_votes", x => new { x.review_id, x.user_id });
                    table.ForeignKey(
                        name: "FK_review_votes_reviews_review_id",
                        column: x => x.review_id,
                        principalTable: "reviews",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_review_votes_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "categories",
                columns: new[] { "id", "created_at", "description", "image", "is_active", "name", "parent_id", "slug", "sort_order", "updated_at" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), "Các loại cây phù hợp trồng trong nhà, văn phòng.", "https://res.cloudinary.com/dvsqjznt2/image/upload/v1761620174/trong-cay-van-phong-cay-trong-van-phong-dep-1_otymq5.jpg", true, "Cây Trong Nhà", null, "cay-trong-nha", 1, new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc) },
                    { 2, new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), "Các loại cây cảnh, cây ăn quả trồng ngoài trời.", "https://res.cloudinary.com/dvsqjznt2/image/upload/v1761620241/147202426979463-collage_tl9zw4.jpg", true, "Cây Ngoài Trời", null, "cay-ngoai-troi", 2, new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "coupon_templates",
                columns: new[] { "id", "created_at", "description", "discount_type", "discount_value", "is_active", "min_order_amount", "name", "points_cost", "total_usage_limit", "usage_limit_per_user", "valid_days" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), "Áp dụng cho đơn hàng tối thiểu 500.000 VNĐ", "PERCENT", 10m, true, 500000m, "Giảm 10% cho đơn hàng trên 500K", 500, 100, 2, 30 },
                    { 2, new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), "Áp dụng cho đơn hàng tối thiểu 1.000.000 VNĐ", "FIXED_AMOUNT", 100000m, true, 1000000m, "Giảm 100K cho đơn hàng trên 1 triệu", 800, 50, 1, 60 }
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "id", "created_at", "role_name" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), "ROLE_ADMIN" },
                    { 2, new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), "ROLE_CUSTOMER" },
                    { 3, new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), "ROLE_STAFF" }
                });

            migrationBuilder.InsertData(
                table: "suppliers",
                columns: new[] { "id", "address", "code", "contact_person", "created_at", "email", "is_active", "name", "payment_terms", "phone", "tax_code", "updated_at" },
                values: new object[,]
                {
                    { 1, "Khu Công Nghiệp Trà Nóc, Cần Thơ", "VX001", "Ms. Lan", new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), "vuonxinh@supplier.com", true, "Nhà Cung Cấp Vườn Xinh", "30 ngày", "02923111222", "1234567890", new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc) },
                    { 2, "12 Đường 3/2, Quận Ninh Kiều, TP. Cần Thơ", "CXABC001", "Mr. Hai", new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), "cayxanh@abc.com", true, "Công ty Cây Xanh ABC", "60 ngày", "02923888999", "9876543210", new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc) },
                    { 3, "Đường 30/4, Quận Cai Rang, TP. Cần Thơ", "NVST001", "Ms. Mai", new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), "sinhthai@nhavuon.com", true, "Nhà Vườn Sinh Thái", "45 ngày", "02923555777", "5555555555", new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "avatar", "created_at", "district", "email", "email_verified_at", "full_name", "password", "phone", "phone_verified_at", "points", "province", "specific_address", "status", "updated_at", "wallet_balance", "ward" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), "Ninh Kieu", "admin@example.com", new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), "Admin User", "Vm9pY2VzQW5kRnVyaWVz:Sv7Awsgb6kcKRMp6O2tJ9Hcc339MpGiw8Cn+BF+D2vuwF/8V63bLOCxwfkaS4j9TLKIHKeJozDBingSJnXw+qw==", "0123456789", new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), 0, "Can Tho", "123 Admin St", "ACTIVE", new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), 0m, "An Khanh" },
                    { 2, null, new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), "Cai Rang", "customer@example.com", new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), "Customer User", "Vm9pY2VzQW5kRnVyaWVz:LNzQLICso8gipi0mT7+EI013y7r9ZKmPb6G0WM5v70zH0nwlN9IQXSuml2XgLALgJaiQt0vtxJBw6Jv4PjWRTA==", "0987654321", new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), 100, "Can Tho", "456 Customer Ave", "ACTIVE", new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), 50000m, "Le Binh" },
                    { 3, null, new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), "Ninh Kieu", "staff@example.com", new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), "Staff User", "Vm9pY2VzQW5kRnVyaWVz:WfPffOpZD2DsxmZ2+l1KKUUcpJX9G9hP05R3bWGaYnE8Y811EnH493YgJ6PhGpP6We7DODJ1fum+bGiJUOJ5Pw==", "0111222333", new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), 0, "Can Tho", "789 Staff Street", "ACTIVE", new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), 0m, "Xuan Khanh" }
                });

            migrationBuilder.InsertData(
                table: "banners",
                columns: new[] { "id", "click_count", "created_at", "created_by", "description", "end_date", "image_url", "is_active", "link_url", "position", "sort_order", "start_date", "title", "updated_at" },
                values: new object[,]
                {
                    { 1, 0, new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), 1, "Nền tảng mua sắm cây cảnh số 1 Việt Nam", new DateTime(2026, 4, 28, 3, 25, 6, 233, DateTimeKind.Utc), "https://res.cloudinary.com/dvsqjznt2/image/upload/v1761619562/Screenshot_2081_gnwkty.png", true, "/#", "HOME_SLIDER", 1, new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), "Chào mừng đến với GreenTech", new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc) },
                    { 2, 0, new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), 1, "Áp dụng cho khách hàng mới", new DateTime(2026, 1, 28, 3, 25, 6, 233, DateTimeKind.Utc), "https://res.cloudinary.com/dvsqjznt2/image/upload/v1761618541/Baner_m0uvff.jpg", true, "/#", "HOME_SLIDER", 2, new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), "Giảm giá 20% cho đơn đầu tiên", new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "blogs",
                columns: new[] { "id", "author_id", "category_id", "content", "created_at", "excerpt", "featured_image", "is_featured", "is_published", "published_at", "seo_description", "seo_title", "slug", "tags", "title", "updated_at", "view_count" },
                values: new object[,]
                {
                    { 1, 1, 1, "Nội dung chi tiết về cách chăm sóc cây cảnh...", new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), "Hướng dẫn chi tiết cách chăm sóc cây cảnh trong nhà để cây luôn xanh tươi, khỏe mạnh.", "https://res.cloudinary.com/dvsqjznt2/image/upload/v1761618345/cach-cham-soc-cay-xanh-trong-nha-3_jwt3pb.webp", true, true, new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), "Tổng hợp những bí quyết chăm sóc cây cảnh trong nhà hiệu quả, giúp không gian sống thêm xanh.", "Cách chăm sóc cây cảnh trong nhà - Hướng dẫn chi tiết", "cach-cham-soc-cay-canh-trong-nha", "cham-soc-cay,cay-trong-nha,meo-hay", "Cách chăm sóc cây cảnh trong nhà", new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), 0 },
                    { 2, 1, 1, "Nội dung chi tiết về các loại cây phong thủy...", new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), "Khám phá 10 loại cây phong thủy mang lại tài lộc, may mắn cho gia đình.", "https://res.cloudinary.com/dvsqjznt2/image/upload/v1761618409/top-19-loai-cay-canh-trong-nha-hop-phong-thuy-va-de-cham-soc-nhat-hien-nay-651645fae15e8c8b38af38ad_flrmui.webp", false, true, new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), "Danh sách các loại cây phong thủy nên trồng trong nhà để mang lại may mắn và tài lộc.", "Top 10 cây phong thủy - Mang tài lộc vào nhà", "top-10-loai-cay-phong-thuy", "phong-thuy,cay-phong-thuy,tai-loc", "Top 10 loại cây phong thủy", new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), 0 }
                });

            migrationBuilder.InsertData(
                table: "carts",
                columns: new[] { "id", "coupon_id", "created_at", "discount_amount", "session_id", "subtotal", "total_items", "updated_at", "user_id" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), 0m, null, 0m, 0, new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), 1 },
                    { 2, null, new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), 0m, null, 0m, 0, new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), 2 },
                    { 3, null, new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), 0m, null, 0m, 0, new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), 3 }
                });

            migrationBuilder.InsertData(
                table: "coupons",
                columns: new[] { "id", "code", "created_at", "discount_type", "discount_value", "end_date", "is_active", "min_order_amount", "name", "points_used", "source", "start_date", "template_id", "usage_limit", "used_count", "user_id" },
                values: new object[,]
                {
                    { 1, "WELCOME10", new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), "PERCENT", 10m, new DateTime(2025, 12, 27, 3, 25, 6, 233, DateTimeKind.Utc), true, 500000m, "Chào mừng giảm 10%", 0, "SYSTEM", new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), 1, 100, 0, null },
                    { 2, "VIP100K", new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), "FIXED_AMOUNT", 100000m, new DateTime(2026, 1, 26, 3, 25, 6, 233, DateTimeKind.Utc), true, 1000000m, "VIP giảm 100K", 0, "PROMOTION", new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), 2, 50, 0, null }
                });

            migrationBuilder.InsertData(
                table: "notifications",
                columns: new[] { "id", "created_at", "is_read", "message", "priority", "read_at", "reference_id", "title", "type", "user_id" },
                values: new object[] { 1, new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), false, "Cảm ơn bạn đã đăng ký tài khoản! Bạn có 10.000 điểm thưởng.", "MEDIUM", null, null, "Chào mừng đến với GreenTech", "SYSTEM", 2 });

            migrationBuilder.InsertData(
                table: "point_earning_rules",
                columns: new[] { "id", "created_at", "created_by", "is_active", "max_points_per_order", "min_order_amount", "name", "points_per_amount", "valid_from", "valid_until" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), 1, true, 1000, 0m, "Quy tắc tích điểm chuẩn", 1m, new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), null },
                    { 2, new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), 1, true, 2000, 1000000m, "Khuyến mãi tích điểm x2", 2m, new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), new DateTime(2026, 1, 28, 3, 25, 6, 233, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "products",
                columns: new[] { "id", "care_instructions", "category_id", "cost_price", "created_at", "description", "dimensions", "image", "is_active", "is_featured", "name", "plant_size", "points_earned", "quantity", "sell_price", "seo_description", "seo_title", "short_description", "sku", "slug", "supplier_id", "tags", "updated_at", "weight" },
                values: new object[,]
                {
                    { 1, "Tưới nước vừa phải khi đất khô. Tránh ánh nắng trực tiếp quá gắt.", 1, 200000m, new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), "Cây Lưỡi Hổ (Sansevieria trifasciata) là loại cây phổ biến, dễ chăm sóc, có khả năng lọc bỏ các độc tố trong không khí.", "Chậu 15cm", "https://res.cloudinary.com/dvsqjznt2/image/upload/v1761620341/cay-luoi-ho-ten-khoa-hoc-sansevieria-trifasciata_v0kubp.jpg", true, true, "Cây Lưỡi Hổ", "Nhỏ (30-40cm)", 10, 50, 150000m, "Cây Lưỡi Hổ đẹp, giá tốt, phù hợp trang trí nhà cửa, văn phòng. Giúp thanh lọc không khí hiệu quả.", "Mua Cây Lưỡi Hổ - Lọc Không Khí, Dễ Chăm Sóc", "Cây phong thủy, lọc không khí tốt.", "CLH001", "cay-luoi-ho", 1, "cay-trong-nha,phong-thuy,loc-khong-khi", new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), 1.5m },
                    { 2, "Ưa bóng râm, tưới nước 2-3 lần/tuần. Bón phân định kỳ.", 1, 350000m, new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), "Cây Phát Tài (Dracaena fragrans) hay còn gọi là Thiết Mộc Lan, được tin là mang lại may mắn và tài lộc. Cây có sức sống tốt, dễ trồng.", "Chậu 25cm", "https://res.cloudinary.com/dvsqjznt2/image/upload/v1761620487/ca_CC_81ch-cha_CC_86m-so_CC_81c-ca_CC_82y-pha_CC_81t-ta_CC_80i-de_CC_82_CC_89-trong-nha_CC_80_mmkhjo.jpg", true, false, "Cây Phát Tài", "Trung bình (60-80cm)", 30, 30, 250000m, "Bán cây Phát Tài hợp phong thủy, trang trí nội thất sang trọng. Cây dễ chăm, mang lại vượng khí.", "Cây Phát Tài (Thiết Mộc Lan) - Mang May Mắn, Tài Lộc", "Mang lại may mắn, tài lộc cho gia chủ.", "CPT005", "cay-phat-tai", 1, "cay-trong-nha,phong-thuy,may-man,tai-loc", new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), 3.0m },
                    { 3, "Tưới nước khi đất khô. Không cần ánh sáng trực tiếp. Bón phân mỗi tháng một lần.", 1, 150000m, new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), "Cây Trầu Bà (Epipremnum aureum) là một trong những cây lọc không khí tốt nhất. Cây dễ chăm sóc, phù hợp cho người mới bắt đầu.", "Chậu 12cm", "https://res.cloudinary.com/dvsqjznt2/image/upload/v1761619808/images_vm4li5.jpg", true, false, "Cây Trầu Bà", "Nhỏ (20-30cm)", 8, 80, 80000m, "Cây Trầu Bà đẹp, dễ chăm, giúp thanh lọc không khí trong nhà hiệu quả.", "Mua Cây Trầu Bà - Lọc Không Khí Hiệu Quả", "Cây lọc không khí tuyệt vời, dễ trồng.", "CTB003", "cay-trau-ba", 2, "cay-trong-nha,loc-khong-khi,de-cham", new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), 0.8m },
                    { 4, "Tưới nước 1-2 lần/tuần. Nên đặt nơi có ánh sáng gián tiếp. Lau lá thường xuyên.", 1, 150000m, new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), "Cây Đa Búp Đỏ (Ficus elastica) với lá bóng, xanh mướt, mang lại không gian tươi mát cho phòng làm việc.", "Chậu 15cm", "https://res.cloudinary.com/dvsqjznt2/image/upload/v1761619886/Cay-da-bup-do_p1whyu.png", true, true, "Cây Đa Búp Đỏ", "Nhỏ (25-35cm)", 10, 60, 100000m, "Cây Đa Búp Đỏ đẹp, dễ chăm, phù hợp trang trí bàn làm việc, phòng khách.", "Cây Đa Búp Đỏ - Trang Trí Văn Phòng", "Cây để bàn làm việc đẹp mắt.", "CDB004", "cay-da-bup-do", 3, "cay-de-ban,tin-cay,xanh-mat", new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), 1.2m }
                });

            migrationBuilder.InsertData(
                table: "user_roles",
                columns: new[] { "role_id", "user_id" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 2 },
                    { 3, 3 }
                });

            migrationBuilder.InsertData(
                table: "product_images",
                columns: new[] { "id", "alt_text", "created_at", "image_url", "is_primary", "product_id", "sort_order" },
                values: new object[,]
                {
                    { 1, "Ảnh chi tiết cây Lưỡi Hổ", new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), "https://res.cloudinary.com/dvsqjznt2/image/upload/v1761620780/cay-luoi-ho-2_fxisjk.jpg", false, 1, 1 },
                    { 2, "Chậu cây Lưỡi Hổ", new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), "https://res.cloudinary.com/dvsqjznt2/image/upload/v1761620716/chau-cay-hoa-de-ban-2_apigsc.jpg", false, 1, 2 },
                    { 3, "Thân cây Phát Tài", new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), "https://res.cloudinary.com/dvsqjznt2/image/upload/v1761620845/cay-phat-tai-1-goc-dep_u5pi5c.jpg", false, 2, 1 },
                    { 4, "Cây Trầu Bà nhỏ xinh", new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), "https://res.cloudinary.com/dvsqjznt2/image/upload/v1761620927/trau-ba-mini_jdnmjk.jpg", false, 3, 1 },
                    { 5, "Cây Đa Búp Đỏ đẹp mắt", new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), "https://res.cloudinary.com/dvsqjznt2/image/upload/v1761620976/cay-da-bup-do_b7k0h6.jpg", false, 4, 1 }
                });

            migrationBuilder.InsertData(
                table: "product_rating_stats",
                columns: new[] { "product_id", "average_rating", "last_updated", "star_1_count", "star_2_count", "star_3_count", "star_4_count", "star_5_count", "total_reviews", "with_content_count", "with_media_count" },
                values: new object[,]
                {
                    { 1, 0m, new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), 0, 0, 0, 0, 0, 0, 0, 0 },
                    { 2, 0m, new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), 0, 0, 0, 0, 0, 0, 0, 0 },
                    { 3, 0m, new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), 0, 0, 0, 0, 0, 0, 0, 0 },
                    { 4, 0m, new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc), 0, 0, 0, 0, 0, 0, 0, 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_banners_created_by",
                table: "banners",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_blogs_author_id",
                table: "blogs",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "IX_blogs_category_id",
                table: "blogs",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_blogs_slug",
                table: "blogs",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cart_items_cart_id_product_id",
                table: "cart_items",
                columns: new[] { "cart_id", "product_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cart_items_product_id",
                table: "cart_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_carts_coupon_id",
                table: "carts",
                column: "coupon_id");

            migrationBuilder.CreateIndex(
                name: "IX_carts_user_id",
                table: "carts",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_categories_parent_id",
                table: "categories",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "IX_categories_slug",
                table: "categories",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_coupons_code",
                table: "coupons",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_coupons_template_id",
                table: "coupons",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "IX_coupons_user_id",
                table: "coupons",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_user_id",
                table: "notifications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_items_order_id",
                table: "order_items",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_items_product_id",
                table: "order_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_orders_coupon_id",
                table: "orders",
                column: "coupon_id");

            migrationBuilder.CreateIndex(
                name: "IX_orders_order_number",
                table: "orders",
                column: "order_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_orders_user_id",
                table: "orders",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_point_earning_rules_created_by",
                table: "point_earning_rules",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_point_transactions_point_earning_rule_id",
                table: "point_transactions",
                column: "point_earning_rule_id");

            migrationBuilder.CreateIndex(
                name: "IX_point_transactions_user_id",
                table: "point_transactions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_images_product_id",
                table: "product_images",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_products_category_id",
                table: "products",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_products_sku",
                table: "products",
                column: "sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_products_slug",
                table: "products",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_products_supplier_id",
                table: "products",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "IX_review_replies_review_id",
                table: "review_replies",
                column: "review_id");

            migrationBuilder.CreateIndex(
                name: "IX_review_replies_user_id",
                table: "review_replies",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_review_votes_user_id",
                table: "review_votes",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_order_item_id",
                table: "reviews",
                column: "order_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_product_id",
                table: "reviews",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_user_id",
                table: "reviews",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_suppliers_code",
                table: "suppliers",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_role_id",
                table: "user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_phone",
                table: "users",
                column: "phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_wallet_transactions_order_id",
                table: "wallet_transactions",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_wallet_transactions_user_id",
                table: "wallet_transactions",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "banners");

            migrationBuilder.DropTable(
                name: "blogs");

            migrationBuilder.DropTable(
                name: "cart_items");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "point_transactions");

            migrationBuilder.DropTable(
                name: "product_images");

            migrationBuilder.DropTable(
                name: "product_rating_stats");

            migrationBuilder.DropTable(
                name: "review_replies");

            migrationBuilder.DropTable(
                name: "review_votes");

            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "wallet_transactions");

            migrationBuilder.DropTable(
                name: "carts");

            migrationBuilder.DropTable(
                name: "point_earning_rules");

            migrationBuilder.DropTable(
                name: "reviews");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "order_items");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "coupons");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "suppliers");

            migrationBuilder.DropTable(
                name: "coupon_templates");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
