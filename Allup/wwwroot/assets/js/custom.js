$(document).ready(function () {
    $('.addToBasket').click(function myFunction(e) {
        e.preventDefault();

        let productId = $(this).data('id');

        fetch('basket/AddBasket?id=' + productId)
            .then(res => {
                return res.text();
            }).then(data => {
                console.log(data)
                $('.header-cart').html(data)
            })
    })

    $('#SearchInput').keyup(function ()
    {
        const searchValue = $(this).val();
        const category = $('#Category').find(":selected").val();
        console.log(category)
        if (searchValue.length >= 3) {
            fetch('product/search?search=' + searchValue + '&categoryId=' + category)
                .then(res => {
                    return res.text()
                }).then(data => {
                    $('#SearchBody').html(data)
                })
        } else {
            $('#SearchBody').html('')

        }
    })

    $(".productModal").click(function (e) {
        e.preventDefault();

        let url = $(this).attr('href');

        fetch(url).then(res =>
        {
            return res.text ();
        })
            .then(data => {
                $('.modal-content').html(data)
                //===== slick Slider Product Quick View

                $('.quick-view-image').slick({
                    slidesToShow: 1,
                    slidesToScroll: 1,
                    arrows: false,
                    dots: false,
                    fade: true,
                    asNavFor: '.quick-view-thumb',
                    speed: 400,
                });

                $('.quick-view-thumb').slick({
                    slidesToShow: 4,
                    slidesToScroll: 1,
                    asNavFor: '.quick-view-image',
                    dots: false,
                    arrows: false,
                    focusOnSelect: true,
                    speed: 400,
                });

            })
    })
})