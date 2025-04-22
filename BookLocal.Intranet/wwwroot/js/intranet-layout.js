// Prosty skrypt do przełączania widoczności paska bocznego na mniejszych ekranach
$(document).ready(function () {
    $("#menu-toggle").click(function (e) {
        e.preventDefault();
        $("#wrapper").toggleClass("toggled");
    });

    // Opcjonalnie: Zamknij menu po kliknięciu linku na małych ekranach
    if ($(window).width() <= 768) {
        $(".list-group-item").click(function () {
            if ($("#wrapper").hasClass("toggled")) {
                $("#wrapper").removeClass("toggled");
            }
        });
    }

    // Opcjonalnie: Automatyczne dodanie klasy 'active' do linku w sidebarze
    // na podstawie bieżącego URL. To bardziej zaawansowane, ale przydatne.
    $(function () {
        var currentPath = window.location.pathname;
        // Dokładne dopasowanie ścieżki
        $('#sidebar-wrapper .list-group-item').each(function () {
            var $this = $(this);
            // Sprawdź, czy atrybut href DOKŁADNIE pasuje do ścieżki
            if ($this.attr('href') === currentPath) {
                $this.addClass('active');
            }
        });
        // Jeśli nie ma dokładnego dopasowania, spróbuj dopasować początek ścieżki (np. dla /Products i /Products/Create)
        if (!$('#sidebar-wrapper .list-group-item.active').length) {
            $('#sidebar-wrapper .list-group-item').each(function () {
                var $this = $(this);
                // Sprawdź, czy ścieżka ZACZYNA SIĘ od href linku (i href nie jest tylko '/')
                if ($this.attr('href') !== '/' && currentPath.startsWith($this.attr('href'))) {
                    $this.parents('.collapse').addClass('show'); // Jeśli używasz podmenu zwijanych
                    $this.addClass('active');
                    // Zazwyczaj chcemy tylko jedno aktywne główne menu
                    return false;
                }
            });
        }
        // Jeśli nadal nic nie pasuje (np. jesteśmy na Dashboard /), zaznacz Dashboard
        if (!$('#sidebar-wrapper .list-group-item.active').length) {
            $('#sidebar-wrapper .list-group-item[href="/"]').addClass('active');
            // lub dla Dashboard jeśli ma inny URL:
            // $('#sidebar-wrapper .list-group-item[asp-controller="Dashboard"]').addClass('active');
        }
    });

});