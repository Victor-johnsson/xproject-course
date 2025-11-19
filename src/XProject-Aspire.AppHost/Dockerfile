# Use the official Citus image as base
FROM citusdata/citus:latest

# Set environment variables
ENV POSTGRES_PASSWORD=mypassword

# Expose the PostgreSQL port
EXPOSE 5432

# These variables can be overridden at runtime
ENV POSTGRES_USER=postgres
ENV POSTGRES_DB=postgres

# Add healthcheck to verify the database is running
HEALTHCHECK --interval=30s --timeout=30s --start-period=5s --retries=3 \
    CMD pg_isready -U postgres || exit 1

# Best practices for PostgreSQL in containers
RUN mkdir -p /var/lib/postgresql/data && \
    chown -R postgres:postgres /var/lib/postgresql/data && \
    chmod 700 /var/lib/postgresql/data

WORKDIR /azp/
COPY ./appsettings.json .
COPY ./init.sql /docker-entrypoint-initdb.d/


# Set the default command
CMD ["postgres"]

