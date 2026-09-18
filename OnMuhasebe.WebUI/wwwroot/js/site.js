// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Onceden doldurulmus (oneri kod/no, varsayilan 0/0,00 gibi) metin alanlarina
// tiklaninca/Tab ile gelininde tum icerik secili gelsin. Boylece kullanici
// yazmaya basladiginda imlecin bulundugu yere karisip "C0008C0008" gibi
// anlamsiz degerler olusmaz; uzerine yazmak yeterli olur. Tum inputlara
// tek bir olay dinleyiciyle uygulaniyor, her forma ayri kod eklemeye gerek yok.
document.addEventListener('focusin', function (e) {
    var el = e.target;
    if (el.tagName === 'INPUT' &&
        (el.type === 'text' || el.type === 'number' || el.type === 'tel' || el.type === 'email')) {
        el.select();
    }
});
