-- 1. GROUPS TABLE
create table if not exists groups
(
    id   serial primary key,
    name varchar(50) not null
);

-- 2. PLANTS TABLE
create table if not exists plants
(
    id          serial primary key,
    group_id    integer references groups on delete set null,
    name        varchar(100) not null,
    description varchar(255),
    created_at  timestamp with time zone default now()
);

-- 3. DEVICES TABLE
create table if not exists devices
(
    id               serial primary key,
    mac_address      varchar(17) not null unique,
    -- gen_random_uuid() is built-in for Postgres 15+, perfect choice
    api_token        uuid default gen_random_uuid() not null unique,
    current_plant_id integer references plants on delete set null
);

create index if not exists idx_devices_token on devices (api_token);

-- 4. SENSOR DATA TABLE
create table if not exists sensor_data
(
    id             bigserial primary key,
    plant_id       integer not null references plants on delete cascade,
    moisture_value double precision not null,
    measured_at    timestamp with time zone default now()
);


create index if not exists idx_sensor_plant_time
    on sensor_data (plant_id asc, measured_at desc);