(function () {
    document.querySelectorAll("[data-mascara]").forEach(function (campo) {
        var modelo = campo.getAttribute("data-mascara");
        if (!modelo) return;
        var limite = (modelo.match(/#/g) || []).length;

        campo.addEventListener("input", function () {
            var digitos = campo.value.replace(/\D/g, "").slice(0, limite);
            var indice = 0;
            var saida = "";
            for (var i = 0; i < modelo.length && indice < digitos.length; i++) {
                saida += modelo[i] === "#" ? digitos[indice++] : modelo[i];
            }
            campo.value = saida;
        });
    });
})();
