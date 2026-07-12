from django.db import migrations, models


class Migration(migrations.Migration):

    initial = True

    dependencies = []

    operations = [
        migrations.CreateModel(
            name="Article",
            fields=[
                ("article_id", models.AutoField(primary_key=True, serialize=False)),
                ("name", models.CharField(max_length=64)),
                ("description", models.CharField(max_length=2048)),
                ("category", models.CharField(blank=True, max_length=64, null=True)),
                ("price", models.DecimalField(decimal_places=2, max_digits=18)),
                ("currency", models.CharField(blank=True, max_length=3, null=True)),
                ("version", models.IntegerField(default=1)),
            ],
            options={"db_table": "articles"},
        ),
    ]
