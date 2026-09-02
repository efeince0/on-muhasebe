// jQuery Validate sayilari nokta ayracli bekler; Turkce bicimde virgul gelir.
// Sunucu tarafi tr-TR kulturuyle zaten dogru cozuyor, istemci kurallarini ona uyduruyoruz.
(function () {
    if (!window.jQuery || !window.jQuery.validator) return;

    function sayiyaCevir(deger) {
        return parseFloat(String(deger).replace(/\./g, "").replace(",", "."));
    }

    jQuery.validator.methods.number = function (value, element) {
        return this.optional(element) ||
            /^-?(\d+|\d{1,3}(\.\d{3})+)(,\d+)?$/.test(value);
    };

    jQuery.validator.methods.range = function (value, element, param) {
        var sayi = sayiyaCevir(value);
        return this.optional(element) || (sayi >= param[0] && sayi <= param[1]);
    };

    jQuery.validator.methods.min = function (value, element, param) {
        return this.optional(element) || sayiyaCevir(value) >= param;
    };

    jQuery.validator.methods.max = function (value, element, param) {
        return this.optional(element) || sayiyaCevir(value) <= param;
    };
})();
