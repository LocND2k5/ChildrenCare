# Part 2 - Post va Slider Management (MVC Structure)

## Tong quan
Phan 2 quan ly Bai viet (Post) va Slider anh trang chu.
Ap dung mo hinh MVC (Model - View - Controller).

---

## Cau truc thu muc

Part2/
- Models/                       [TANG MODEL - Du lieu]
    Post.cs                     # Entity bai viet
    Slider.cs                   # Entity slider anh
    PostStatus.cs               # Enum: Draft / Published / Archived
    SliderStatus.cs             # Enum: Active / Inactive

- Controllers/                  [TANG CONTROLLER - Xu ly logic]
    Public/
        HomeController.cs       # Load Slider + bai viet noi bat -> trang chu
        BlogsController.cs      # Danh sach blog + xem chi tiet
    Admin/
        PostsController.cs      # CRUD bai viet (Index, Create, Edit, ToggleStatus)
        SlidersController.cs    # CRUD slider (Index, Create, Edit, ToggleStatus)

- ViewModels/                   [TANG VIEWMODEL - Trung gian Model <-> View]
    Public/
        HomeViewModel.cs        # List<Slider> + List<Post> featured
        BlogListViewModel.cs    # Danh sach blog + phan trang + tim kiem
    Admin/
        PostListViewModel.cs    # Danh sach bai viet Admin (co filter)
        PostFormViewModel.cs    # Form tao/sua bai viet (co validation)
        SliderListViewModel.cs  # Danh sach Slider Admin
        SliderFormViewModel.cs  # Form tao/sua Slider (co validation)

- Views/                        [TANG VIEW - Giao dien Razor .cshtml]
    Public/
        Home/
            Index.cshtml        # Trang chu: Slider carousel + bai viet noi bat
        Blogs/
            Index.cshtml        # Danh sach bai viet (search + phan trang)
            Details.cshtml      # Chi tiet mot bai viet
    Admin/
        Posts/
            Index.cshtml        # Bang quan ly bai viet (filter theo status)
            Form.cshtml         # Form tao/sua bai viet (dung chung)
        Sliders/
            Index.cshtml        # Bang quan ly Slider (preview anh)
            Form.cshtml         # Form tao/sua Slider

---

## Luong MVC
HTTP Request
    -> Controller (nhan request, xu ly logic)
    -> Model / DbContext (lay du lieu tu DB)
    -> ViewModel (dong goi du lieu)
    -> View / Razor (.cshtml) (render HTML tra ve user)

---

## CRUD theo Use Case

Use Case                      | Actor    | Method    | URL
------------------------------|----------|-----------|---------------------------
Xem trang chu                 | Customer | GET       | /
Xem danh sach blog            | Customer | GET       | /Blogs
Xem chi tiet bai viet         | Customer | GET       | /Blogs/Details/{id}
Quan ly bai viet              | Admin    | GET       | /Admin/Posts
Tao bai viet moi              | Admin    | GET+POST  | /Admin/Posts/Create
Chinh sua bai viet            | Admin    | GET+POST  | /Admin/Posts/Edit/{id}
Bat/Tat trang thai bai viet   | Admin    | POST      | /Admin/Posts/ToggleStatus/{id}
Quan ly Slider                | Admin    | GET       | /Admin/Sliders
Them Slider moi               | Admin    | GET+POST  | /Admin/Sliders/Create
Chinh sua Slider              | Admin    | GET+POST  | /Admin/Sliders/Edit/{id}
Bat/Tat Slider                | Admin    | POST      | /Admin/Sliders/ToggleStatus/{id}

---

## Phan quyen

Chuc nang         | Customer | Admin | Manager
------------------|----------|-------|--------
Xem blog          | YES      | YES   | YES
Quan ly bai viet  | NO       | YES   | YES
Quan ly Slider    | NO       | YES   | YES
