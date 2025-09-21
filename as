$.ajax({
    url: '/Report/GetHourlyCounts',
    type: 'GET',
    success: function (response) {
        // response = [{key:"00:00-01:00",value:154}, ...]

        const labels = response.map(x => x.key);   // ["00:00-01:00", "01:00-02:00", ...]
        const values = response.map(x => x.value); // [154, 165, 432, ...]

        const ctx = document.getElementById('myChart').getContext('2d');

        new Chart(ctx, {
            type: 'line',
            data: {
                labels: labels,      // X ekseni: saat aralıkları
                datasets: [{
                    label: 'Saatlik Sayılar',
                    data: values,    // Y ekseni: değerler
                    borderColor: 'rgb(75, 192, 192)',
                    tension: 0.2,
                    fill: false
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    title: {
                        display: true,
                        text: '24 Saatlik Dağılım'
                    }
                },
                scales: {
                    x: {
                        title: {
                            display: true,
                            text: 'Saat Aralığı'
                        }
                    },
                    y: {
                        beginAtZero: true,
                        title: {
                            display: true,
                            text: 'Adet'
                        }
                    }
                }
            }
        });
    }
});
