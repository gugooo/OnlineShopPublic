$(document).ready(function () {
    var pr = $.cookie("FavoritIds") && JSON.parse($.cookie("FavoritIds"));
    pr && pr.length && $('#favoritesWrap small').removeClass('d-none').text(pr.length > 99 ? '99+' : pr.length);

    var prB = $.cookie("ProductList") && JSON.parse($.cookie("ProductList"));
    if ($.isArray(prB)) {
        var prCount = 0;
        $.each(prB, function () { prCount = prCount + this.count; })
        prB && prB.length && $('#ShoppingBagWrap small').removeClass('d-none').text(prCount > 99 ? '99+' : prCount);
    }
    $('#SubscribeButton').click(function () {
        if ($('#SubscribeNewEmail').val()) {
            $.post('/Home/Subscribe?email=' + $('#SubscribeNewEmail').val(), function (data) {
                if (data && data.res) {
                    $('#SubscribeNewEmail').addClass('bg-success').removeClass('bg-danger');
                }
                else $('#SubscribeNewEmail').removeClass('bg-success').addClass('bg-danger');
            });
        }
        else $('#SubscribeNewEmail').removeClass('bg-success').addClass('bg-danger');
    }); 
    $(".HeartSection").ready(function () {
        var ids = $.cookie("FavoritIds") && JSON.parse($.cookie("FavoritIds"));
        var prs = $("#productList .ProductWrapper");
        if ($.isArray(ids) && prs.length > 0) {
            $.each(ids, function (i, at) {
                $.each(prs, function (j, pr) {
                    if (at.id === $(pr).data('id')) {
                        $(pr).find(".Heart").addClass('HeartSelected');
                        //return false;
                    }
                });
            });
        }
    });
    $(".HeartSection").click(function (e) {
        if ($(e.target).hasClass("Heart") || $(e.target).is('.lar, .la-heart')) {
            var el = $(e.target);
            if ($(el).is('.lar, .la-heart')) el = $(el).parent();
            if ($(el).hasClass("HeartSelected")) {
                $(el).removeClass("HeartSelected");
                var ids = $.cookie("FavoritIds");
                ids = ids && JSON.parse(ids);
                if ($.isArray(ids)) {
                    ids = $.grep(ids, function (at) { return at.id != $(el).parent().data('id'); });
                    $.cookie("FavoritIds", JSON.stringify(ids), { expires: 365, path: '/' });
                    ids.length ? (ids.length > 99 ? $('#favoritesWrap small').text('99+') : $('#favoritesWrap small').text(ids.length)) : $('#favoritesWrap small').addClass('d-none');
                }
            }
            else {
                $(el).addClass("HeartSelected");
                var ids = $.cookie("FavoritIds");
                if (!ids) {
                    ids = [{ id: $(el).parent().data('id'), atrId: $(el).parent().data('atrid') }];
                    $.cookie("FavoritIds", JSON.stringify(ids), { expires: 365, path: '/' });
                    $('#favoritesWrap small').removeClass('d-none').text(1);

                }
                else {
                    ids = JSON.parse(ids);
                    if ($.isArray(ids)) {
                        ids.push({ id: $(el).parent().data('id'), atrId: $(el).parent().data('atrid') });
                        $.cookie("FavoritIds", JSON.stringify(ids), { expires: 365, path: '/' });
                        ids.length > 99 ? $('#favoritesWrap small').text('99+') : $('#favoritesWrap small').text(ids.length);
                        if (ids.length == 1) $('#favoritesWrap small').removeClass('d-none');
                    }
                }
            }
        }
    });
    $(".QVLissener").click(function (e) {
        if ($(e.target).hasClass("qv") || $(e.target).is('.las, .la-eye')) {
            var el = $(e.target);
            if ($(el).is('.las, .la-eye')) el = $(el).closest('.qv');
            var id = el.data('id');

            $.post("/Home/QuickView/" + id, function (data) {
                if (data && data.res) {
                    var res = data.res;
                    $("#quick-view img").attr("src", res.img_src);
                    $("#quick-view .BuyNow").attr("href", res.buy_src);
                    $("#quick-view .GetPr").attr("href", res.pr_src);
                    $("#quick-view .AddToCart").data("id", res.id);
                    $("#quick-view .prTitle").html(res.title);
                    $("#quick-view .prDes").html(res.desc);
                    $("#quick-view .prBrand").text(res.brand);
                    $("#quick-view .star-rating").empty().append('<i class="las la-star"></i>'.repeat(res.rating) + '<i class="las la-star font-weight-light"></i>'.repeat(5 - res.rating));
                    $("#quick-view .product-price").empty().append((res.sale && res.sale != '0' ? ('<del class="text-muted">' + res.price + '</del><b> ' + res.sale + '</b>') : ('<b>' + res.price + '</b>')) + '<small>' + res.amd+'</small>');


                }
            });
        }
    });
    updateCart();
    $('#liveChatStart').click(function () {
        if (!$('#startChatWrap textarea').val()) {
            $('#startChatWrap textarea').addClass('border-danger').focus();
            event.stopPropagation();
            return false;
        }
        $('#startChatWrap').addClass('d-none');
        $('#messagingChatWrap').removeClass('d-none');
        $('#startChatWrap textarea').removeClass('border-danger');
        HubBild();
    });
});

function popupCenter(url, title, w, h) {
    // Fixes dual-screen position                             Most browsers      Firefox
    //const dualScreenLeft = window.screenLeft !== undefined ? window.screenLeft : window.screenX;
    //const dualScreenTop = window.screenTop !== undefined ? window.screenTop : window.screenY;

    const width = window.innerWidth ? window.innerWidth : document.documentElement.clientWidth ? document.documentElement.clientWidth : screen.width;
    const height = window.innerHeight ? window.innerHeight : document.documentElement.clientHeight ? document.documentElement.clientHeight : screen.height;

    //const systemZoom = width / window.screen.availWidth;
    const left = (width - w) / 2;// / systemZoom + dualScreenLeft;
    const top = (height - h) / 2;// / systemZoom + dualScreenTop;
    const newWindow = window.open(url, title,
        `
      scrollbars=yes,
      width=${w}, 
      height=${h}, 
      top=${top}, 
      left=${left}
      `
    )
    return newWindow;
}

function googleLogin() {
    var newPop = popupCenter("/Account/GoogleLogin", "Google", 600, 600);
    var timer = setInterval(function () {
        if (newPop.closed) {
            clearInterval(timer);
            location.reload();
        }
    }, 1000);
}
function facebookLogin() {
    var newPop = popupCenter("/Account/FacebookLogin", "Facebook", 600, 600);
    var timer = setInterval(function () {
        if (newPop.closed) {
            clearInterval(timer);
            location.reload();
        }
    }, 1000);
}

function deleteCarpPr(el) {
    var products = $.cookie("ProductList");
    products = products && JSON.parse(products);
    if (products && products.length > 0) {
        var prId = $(el).closest('.CProduct').data('id') + "";
        var atrValIds = $(el).closest('.CProduct .atrVal').data('ids');

        var newProducts = $.grep(products, function (p) { return p.id != prId || $(p.atrs).not(atrValIds).length != 0 });
        if (newProducts.length != products.length) {
            $.cookie("ProductList", JSON.stringify(newProducts), { expires: 365, path: '/' });
            prCount = 0;
            $.each(newProducts, function () { prCount = prCount + this.count; })
            $('#ShoppingBagWrap small').text(prCount > 99 ? '99+' : prCount);
        }
    }
    updateCart();
}
function updateCart() {
    $("#ModalLabel span").text("");
    $("#cartProductsWrap").empty().append('<div class="text-center mt-4 w-100"><div class="spinner-border" role="status"><span class="sr-only">Loading...</span></div></div>');
    $("#cartBodyTotal").text("");
    $.post('/Home/GetCartJson/', function (data) {
        if (data && data.resp) {
            $("#cartProductsWrap").empty();
            $("#ModalLabel span").text(data.count);
            $("#cartBodyTotal").text(data.total);
            $.each(data.resp, function (i, d) {
                var atrs = $('<div class="col-5 d-flex align-items-center atrVal"><div class="mr-4"><button type="button" onclick="deleteCarpPr(this)" class="btn btn-primary btn-sm"><i class="las la-times"></i></button></div>' +
                    '<a href="/Home/Product/' + d.id + '"><img class="img-fluid" src="/Home/GetProductImg/' + d.img + '" alt="..."></a></div>');
                var pr = $('<div><div class="row align-items-center CProduct" data-id="' + d.id + '">' +
                    '<div class="col-7"><h6><a class="link-title" href="/Home/Product/' + d.id + '">' + d.t + ' </a></h6><div class="product-meta"><span class="mr-2 text-primary">' + d.price + '<small> ' + data.asd + '</small></span><span class="text-muted">x ' + d.n + '</span></div></div>'
                    + '</div></div><hr class="my-5">');
                atrs.data("ids", d.atrs);
                pr.find('.CProduct').prepend(atrs);
                $('#cartProductsWrap').append(pr);
            });
        }
        else $("#cartProductsWrap").empty();
    });
}
function addToCart(el) {
    var productId;
    if ($(el).hasClass("AddToCart")) productId = $(el).data('id');
    else productId = $(el).closest('.product-card').data('id');
    var optionsVal = ["0"];
        var products = $.cookie("ProductList");
        products = products && JSON.parse(products);
        if ($.isArray(products)) {
            var productIsAdd = products.find(_ => _.id == productId);
            if (productIsAdd && $(productIsAdd.atrs).not(optionsVal).length == 0 && $(optionsVal).not(productIsAdd.atrs).length == 0) {
                productIsAdd.count = productIsAdd.count + 1;
                products = $.grep(products, function (at) { return at.id != productId; });
                products.push(productIsAdd);
                $.cookie("ProductList", JSON.stringify(products), { expires: 365, path: '/' });
                prCount = 0;
                $.each(products, function () { prCount = prCount + this.count; })
                $('#ShoppingBagWrap small').removeClass('d-none').text(prCount > 99 ? '99+' : prCount);
            }
            else {
                products.push({ id: productId, count: 1, atrs: optionsVal });
                $.cookie("ProductList", JSON.stringify(products), { expires: 365, path: '/' });
                prCount = 0;
                $.each(products, function () { prCount = prCount + this.count; })
                $('#ShoppingBagWrap small').removeClass('d-none').text(prCount);
            }
        }
        else {
            products = [{ id: productId, count: 1, atrs: optionsVal }];
            $.cookie("ProductList", JSON.stringify(products), { expires: 365, path: '/' });
            $('#ShoppingBagWrap small').removeClass('d-none').text(1);
        }
        updateCart();
}

function HubBild() {
    const hubConnection = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub")
        .build();

    hubConnection.on("newMessage", function (name, message) {
        createOpMessage(name, message);
        $('#messagingChat').animate({ scrollTop: $('#messagingChat')[0].scrollHeight }, 1000);
    });

    $("#sendNewMessage").click(function (e) {
        var message = $("#newMessage").val();
        if (!message) {
            $('#newMessage').focus();
            event.stopPropagation();
            return false;
        }
        var name = $('#chatName').val() || "User";
        $("#newMessage").val("");
        createMessage(message);
        $('#messagingChat').animate({ scrollTop: $('#messagingChat')[0].scrollHeight }, 1000);
        hubConnection.invoke("GetMessage", name, message);
    });
    $("#abortChat").click(function () {
        $('#chatName').val("");
        $('#liveChatUserPhone').val("");
        $('#liveChatUserEmail').val("");
        $('#startChatWrap textarea').val("");
        $('#newMessage').val("");
        $('#messagingChat').empty();
        $('#messagingChatWrap').addClass('d-none');
        $('#startChatWrap').removeClass('d-none');
        hubConnection.stop();
    });
    function createMessage(message) {
        var name = $('#chatName').val() || "User";
        var dn = new Date();
        var timeNow = ((dn.getHours() < 10) ? "0" : "") + dn.getHours() + ":" + ((dn.getMinutes() < 10) ? "0" : "") + dn.getMinutes();
        return $('#messagingChat').append('<div class="userMessage"><div><b>' + name + '</b> <span>' + timeNow + '</span><br/><span>' + message + '</span></div></div>');
    }
    function createOpMessage(name, message) {
        var dn = new Date();
        var timeNow = ((dn.getHours() < 10) ? "0" : "") + dn.getHours() + ":" + ((dn.getMinutes() < 10) ? "0" : "") + dn.getMinutes();
        $('#messagingChat').append('<div class="operatorMessage"><div><b>' + name + '</b> <span>' + timeNow + '</span><br/><span>' + message + '</span></div></div>');
    }
    function connectData() {
        var name = $('#chatName').val() || "User";
        hubConnection.invoke("GetMessage", name, ("Phone: " + ($('#liveChatUserPhone').val() ? $('#liveChatUserPhone').val() : "") + ", Email: " + ($('#liveChatUserEmail').val() ? $('#liveChatUserEmail').val() : "")));
        createMessage($('#startChatWrap textarea').val());
        hubConnection.invoke("GetMessage", name, $('#startChatWrap textarea').val());
    }
    hubConnection.start().then(connectData);

}