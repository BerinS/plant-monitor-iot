--
-- PostgreSQL database dump
--

\restrict iOYclZQjIyWyhO2b28gJTTfwiYaeR7KE34ACCQlgYXcdGm98h8jfXTwbdRWnVtj

-- Dumped from database version 18.1
-- Dumped by pg_dump version 18.1

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: devices; Type: TABLE; Schema: public; 
--

CREATE TABLE public.devices (
    id integer NOT NULL,
    mac_address character varying(17) NOT NULL,
    api_token uuid DEFAULT gen_random_uuid() NOT NULL,
    current_plant_id integer
);



--
-- Name: devices_id_seq; Type: SEQUENCE; Schema: public;
--

CREATE SEQUENCE public.devices_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;



--
-- Name: devices_id_seq; Type: SEQUENCE OWNED BY; Schema: public; 
--

ALTER SEQUENCE public.devices_id_seq OWNED BY public.devices.id;


--
-- Name: groups; Type: TABLE; Schema: public; 
--

CREATE TABLE public.groups (
    id integer NOT NULL,
    name character varying(50) NOT NULL
);


--
-- Name: groups_id_seq; Type: SEQUENCE; Schema: public; 
--

CREATE SEQUENCE public.groups_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;



--
-- Name: groups_id_seq; Type: SEQUENCE OWNED BY; Schema: public; 
--

ALTER SEQUENCE public.groups_id_seq OWNED BY public.groups.id;


--
-- Name: plants; Type: TABLE; Schema: public;
--

CREATE TABLE public.plants (
    id integer NOT NULL,
    group_id integer,
    name character varying(100) NOT NULL,
    description character varying(255),
    created_at timestamp with time zone DEFAULT now()
);


--
-- Name: plants_id_seq; Type: SEQUENCE; Schema: public; 
--

CREATE SEQUENCE public.plants_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;



--
-- Name: plants_id_seq; Type: SEQUENCE OWNED BY; Schema: public; 
--

ALTER SEQUENCE public.plants_id_seq OWNED BY public.plants.id;


--
-- Name: sensor_data; Type: TABLE; Schema: public; 
--

CREATE TABLE public.sensor_data (
    id bigint NOT NULL,
    plant_id integer NOT NULL,
    moisture_value double precision NOT NULL,
    measured_at timestamp with time zone DEFAULT now()
);




--
-- Name: sensor_data_id_seq; Type: SEQUENCE; Schema: public; 
--

CREATE SEQUENCE public.sensor_data_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQU

--
-- Name: sensor_data_id_seq; Type: SEQUENCE OWNED BY; Schema: public; 
--

ALTER SEQUENCE public.sensor_data_id_seq OWNED BY public.sensor_data.id;


--
-- Name: devices id; Type: DEFAULT; Schema: public; 
--

ALTER TABLE ONLY public.devices ALTER COLUMN id SET DEFAULT nextval('public.devices_id_seq'::regclass);


--
-- Name: groups id; Type: DEFAULT; Schema: public; 
--

ALTER TABLE ONLY public.groups ALTER COLUMN id SET DEFAULT nextval('public.groups_id_seq'::regclass);


--
-- Name: plants id; Type: DEFAULT; Schema: public; 
--

ALTER TABLE ONLY public.plants ALTER COLUMN id SET DEFAULT nextval('public.plants_id_seq'::regclass);


--
-- Name: sensor_data id; Type: DEFAULT; Schema: public; 
--

ALTER TABLE ONLY public.sensor_data ALTER COLUMN id SET DEFAULT nextval('public.sensor_data_id_seq'::regclass);


--
-- Data for Name: devices; Type: TABLE DATA; Schema: public; 
--

INSERT INTO public.devices (id, mac_address, api_token, current_plant_id) VALUES (1, 'AA:BB:CC:DD:EE:FF', '55555555-aaaa-bbbb-cccc-1234567890ab', 1);
INSERT INTO public.devices (id, mac_address, api_token, current_plant_id) VALUES (2, 'GG:HH:II:JJ:KK:LL', '02812283-f09b-4c68-b921-127286265182', 2);


--
-- Data for Name: groups; Type: TABLE DATA; Schema: public; 
--

INSERT INTO public.groups (id, name) VALUES (1, 'Bedroom');


--
-- Data for Name: plants; Type: TABLE DATA; Schema: public; 
--

INSERT INTO public.plants (id, group_id, name, description, created_at) VALUES (1, 1, 'Peace Lily', 'Small houseplant on the bedroom shelf', '2025-12-15 10:11:53.128398+01');
INSERT INTO public.plants (id, group_id, name, description, created_at) VALUES (2, 1, 'Ficus', 'Tall ficus next to the bed, in the large red vase', '2026-01-07 15:31:11.526214+01');


--
-- Data for Name: sensor_data; Type: TABLE DATA; Schema: public; 
--

INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (228, 1, 95, '2025-12-20 13:12:44.963581+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (229, 1, 96, '2025-12-20 13:25:44.438989+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (230, 1, 97, '2025-12-20 13:40:44.957819+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (231, 1, 96, '2025-12-20 13:55:45.156361+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (232, 1, 96, '2025-12-20 14:10:45.300394+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (233, 1, 95, '2025-12-20 14:25:46.026022+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (234, 1, 96, '2025-12-20 14:40:46.121086+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (235, 1, 96, '2025-12-20 14:55:46.228899+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (236, 1, 96, '2025-12-20 15:10:46.324606+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (237, 1, 92, '2025-12-21 02:45:33.591977+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (238, 1, 94, '2025-12-21 02:45:51.001332+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (239, 1, 91, '2025-12-21 02:46:10.146663+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (240, 1, 32, '2025-12-21 16:13:11.697967+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (241, 1, 34, '2025-12-21 16:13:13.336268+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (242, 1, 31, '2025-12-21 16:13:32.57024+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (243, 1, 30, '2025-12-21 16:14:46.177691+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (244, 1, 30, '2025-12-21 16:14:48.408256+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (245, 1, 30, '2025-12-21 16:19:07.697301+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (246, 1, 31, '2025-12-21 16:19:24.858215+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (247, 1, 29, '2025-12-21 16:19:40.834233+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (248, 1, 100, '2025-12-21 16:20:42.684083+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (249, 1, 100, '2025-12-21 16:35:42.778803+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (250, 1, 100, '2025-12-21 16:50:42.873768+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (251, 1, 100, '2025-12-21 17:05:42.967392+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (252, 1, 100, '2025-12-21 17:20:43.159762+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (253, 1, 100, '2025-12-21 17:35:43.431712+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (254, 1, 100, '2025-12-21 17:50:43.518447+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (255, 1, 99, '2025-12-21 18:05:43.613323+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (256, 1, 100, '2025-12-21 18:20:43.702195+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (257, 1, 100, '2025-12-21 18:35:43.848858+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (258, 1, 99, '2025-12-21 18:50:43.950626+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (259, 1, 100, '2025-12-21 19:05:44.037464+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (260, 1, 100, '2025-12-21 19:20:44.358659+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (261, 1, 99, '2025-12-21 19:29:00.088836+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (262, 1, 0, '2025-12-28 21:50:17.660889+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (263, 1, 0, '2025-12-28 21:50:39.915306+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (264, 1, 29, '2025-12-28 21:51:28.678796+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (265, 1, 38, '2025-12-28 21:51:54.728949+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (266, 1, 34, '2025-12-28 21:52:20.974656+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (267, 1, 30, '2025-12-28 21:58:19.783583+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (268, 1, 28, '2025-12-28 21:58:59.767216+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (269, 1, 89, '2025-12-28 21:59:53.122118+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (270, 1, 96, '2025-12-28 22:00:21.061499+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (271, 1, 65, '2026-01-06 13:48:48.707757+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (272, 1, 61, '2026-01-06 14:03:50.452912+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (273, 1, 64, '2026-01-06 14:18:51.311024+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (274, 1, 68, '2026-01-06 14:33:51.456065+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (275, 1, 55, '2026-01-06 14:48:53.146166+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (276, 1, 59, '2026-01-06 15:03:53.452226+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (277, 1, 59, '2026-01-06 15:18:53.877365+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (278, 1, 66, '2026-01-06 15:19:49.098544+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (279, 1, 63, '2026-01-06 15:19:51.88811+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (280, 1, 59, '2026-01-06 16:20:23.22968+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (281, 1, 52, '2026-01-06 16:35:23.522674+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (282, 1, 53, '2026-01-06 16:50:23.689437+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (283, 1, 73, '2026-01-06 17:05:24.170546+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (284, 1, 65, '2026-01-06 17:20:24.712739+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (285, 1, 61, '2026-01-06 17:35:24.833056+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (286, 1, 59, '2026-01-06 19:05:50.002287+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (287, 1, 64, '2026-01-06 19:20:50.496179+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (288, 1, 54, '2026-01-06 19:35:50.581445+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (289, 1, 61, '2026-01-06 19:50:50.717672+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (316, 1, 55, '2026-01-07 17:22:38.474084+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (317, 2, 25, '2026-01-07 17:34:14.426267+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (318, 1, 51, '2026-01-07 17:34:39.285978+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (319, 1, 55, '2026-01-07 17:49:39.777374+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (320, 1, 54, '2026-01-07 18:04:39.872855+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (321, 1, 54, '2026-01-08 16:12:45.314557+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (322, 1, 51, '2026-01-08 16:13:19.543527+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (323, 1, 45, '2026-01-08 16:28:19.680323+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (324, 1, 54, '2026-01-08 16:43:19.991501+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (325, 1, 50, '2026-01-08 16:58:20.312914+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (326, 1, 47, '2026-01-08 17:13:20.404358+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (327, 1, 45, '2026-01-08 17:28:20.582124+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (328, 1, 48, '2026-01-08 17:43:20.990995+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (329, 1, 53, '2026-01-08 17:58:21.08266+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (330, 1, 42, '2026-01-08 18:13:21.144713+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (331, 1, 47, '2026-01-08 18:28:21.240796+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (332, 1, 43, '2026-01-08 18:43:21.342814+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (333, 1, 49, '2026-01-08 18:58:21.50561+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (334, 1, 49, '2026-01-08 19:13:21.85199+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (335, 1, 44, '2026-01-08 19:28:22.12313+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (336, 1, 45, '2026-01-08 19:43:22.467294+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (337, 1, 45, '2026-01-08 19:58:22.542232+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (338, 1, 48, '2026-01-08 20:13:22.682968+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (339, 1, 40, '2026-01-08 20:28:22.738292+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (340, 1, 46, '2026-01-08 20:43:22.815059+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (341, 1, 37, '2026-01-08 20:58:22.909076+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (342, 1, 36, '2026-01-08 21:13:22.974943+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (343, 1, 42, '2026-01-08 21:28:23.035731+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (344, 1, 45, '2026-01-08 21:43:23.255393+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (345, 1, 46, '2026-01-08 21:58:23.376053+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (346, 2, 20, '2026-01-08 22:09:14.39716+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (347, 2, 18, '2026-01-08 22:09:21.341603+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (348, 2, 24, '2026-01-08 22:09:24.750333+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (349, 2, 15, '2026-01-08 22:09:29.689007+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (350, 2, 17, '2026-01-08 22:09:33.131594+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (351, 2, 17, '2026-01-08 22:11:07.878659+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (352, 2, 22, '2026-01-08 22:11:17.680661+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (353, 2, 20, '2026-01-08 22:11:27.922419+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (354, 2, 20, '2026-01-08 22:12:03.21636+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (355, 1, 47, '2026-01-08 22:13:23.438745+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (356, 1, 43, '2026-01-08 22:28:23.76828+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (357, 1, 42, '2026-01-08 22:43:23.837067+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (358, 1, 40, '2026-01-08 22:58:24.015553+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (359, 1, 37, '2026-01-08 23:13:24.099481+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (360, 1, 46, '2026-01-08 23:28:27.138176+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (361, 1, 41, '2026-01-08 23:43:27.395229+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (362, 1, 43, '2026-01-08 23:58:27.702207+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (363, 1, 46, '2026-01-09 00:13:27.844295+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (364, 1, 39, '2026-01-09 00:28:27.909157+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (365, 1, 52, '2026-01-09 00:43:28.013589+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (366, 1, 51, '2026-01-09 00:58:28.105005+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (367, 1, 45, '2026-01-09 10:45:34.938131+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (368, 1, 38, '2026-01-09 11:00:36.843337+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (369, 1, 40, '2026-01-09 11:15:37.11281+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (370, 1, 42, '2026-01-09 11:30:37.422377+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (371, 1, 52, '2026-01-09 11:45:37.725828+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (372, 1, 43, '2026-01-09 12:00:37.827499+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (373, 1, 45, '2026-01-09 12:15:37.89473+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (374, 1, 33, '2026-01-09 12:30:38.252291+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (375, 1, 40, '2026-01-09 12:45:41.832442+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (376, 1, 44, '2026-01-09 13:00:42.57645+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (377, 1, 35, '2026-01-09 13:15:43.002606+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (378, 1, 32, '2026-01-09 13:30:43.268348+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (379, 1, 48, '2026-01-09 13:45:43.873787+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (380, 1, 29, '2026-01-09 14:00:44.18058+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (381, 1, 40, '2026-01-09 14:15:44.282266+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (382, 1, 37, '2026-01-09 14:30:44.547064+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (383, 1, 28, '2026-01-09 14:45:45.523919+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (384, 1, 41, '2026-01-09 15:00:46.249165+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (385, 1, 34, '2026-01-09 15:15:46.343714+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (386, 1, 37, '2026-01-09 15:30:46.475474+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (387, 1, 47, '2026-01-09 15:45:47.154381+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (388, 1, 33, '2026-01-09 16:00:47.880477+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (389, 1, 39, '2026-01-09 16:15:48.628114+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (390, 1, 44, '2026-01-09 16:30:48.90104+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (391, 1, 30, '2026-01-09 16:45:49.34841+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (392, 1, 36, '2026-01-09 17:00:49.547053+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (393, 1, 38, '2026-01-09 17:15:52.454205+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (394, 1, 41, '2026-01-09 17:30:52.561912+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (395, 1, 45, '2026-01-09 17:45:52.890898+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (396, 1, 45, '2026-01-09 18:00:53.199244+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (397, 1, 40, '2026-01-09 18:15:53.297402+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (398, 1, 41, '2026-01-09 18:30:53.377874+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (399, 1, 41, '2026-01-09 18:45:53.473664+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (400, 1, 38, '2026-01-09 19:00:53.610531+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (401, 1, 35, '2026-01-09 19:15:53.705805+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (402, 1, 37, '2026-01-09 19:30:53.833433+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (403, 1, 37, '2026-01-09 19:45:54.231847+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (404, 1, 33, '2026-01-09 20:00:54.836609+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (405, 1, 46, '2026-01-09 20:15:54.924331+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (406, 1, 39, '2026-01-09 20:30:55.007015+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (407, 1, 28, '2026-01-09 20:45:55.097456+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (408, 1, 34, '2026-01-09 21:00:55.25005+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (409, 1, 41, '2026-01-09 21:15:55.349661+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (410, 1, 43, '2026-01-09 21:30:55.472412+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (411, 1, 31, '2026-01-09 21:45:55.727672+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (412, 1, 34, '2026-01-09 22:00:56.065162+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (413, 1, 33, '2026-01-09 22:15:56.37602+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (414, 1, 35, '2026-01-09 22:30:56.69631+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (415, 1, 35, '2026-01-09 22:45:56.906972+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (416, 1, 34, '2026-01-09 23:00:57.314106+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (417, 1, 26, '2026-01-09 23:15:57.751752+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (418, 1, 33, '2026-01-09 23:30:57.915529+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (419, 1, 34, '2026-01-09 23:45:58.045898+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (420, 1, 41, '2026-01-10 00:00:58.527252+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (421, 1, 38, '2026-01-10 00:15:58.829694+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (422, 1, 42, '2026-01-10 00:30:59.180146+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (423, 1, 29, '2026-01-10 00:45:59.452163+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (424, 1, 43, '2026-01-10 01:00:59.798999+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (425, 1, 32, '2026-01-10 01:16:00.094559+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (426, 1, 7, '2026-01-12 14:57:05.59107+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (427, 1, 19, '2026-01-12 15:12:06.16503+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (428, 1, 14, '2026-01-12 15:20:40.604326+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (429, 1, 12, '2026-01-12 15:21:27.165647+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (430, 1, 75, '2026-01-12 15:26:58.047278+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (431, 1, 77, '2026-01-12 15:31:06.118645+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (432, 1, 77, '2026-01-12 15:46:06.209551+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (433, 1, 84, '2026-01-12 16:01:06.304955+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (434, 1, 92, '2026-01-12 16:16:06.385209+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (435, 1, 80, '2026-01-12 16:31:06.778667+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (436, 1, 77, '2026-01-12 16:46:06.884764+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (437, 1, 93, '2026-01-12 17:01:06.974617+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (438, 1, 94, '2026-01-12 17:16:07.170268+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (439, 1, 85, '2026-01-12 17:31:07.589657+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (440, 1, 79, '2026-01-12 17:46:07.677331+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (441, 1, 79, '2026-01-12 18:01:07.787703+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (442, 1, 96, '2026-01-12 18:16:07.87589+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (443, 1, 85, '2026-01-12 18:31:07.973211+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (444, 1, 95, '2026-01-12 18:46:08.085688+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (445, 1, 75, '2026-01-12 19:01:08.337828+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (446, 1, 93, '2026-01-12 19:16:08.421926+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (447, 1, 71, '2026-01-12 19:31:08.506224+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (448, 1, 87, '2026-01-12 19:46:08.592024+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (449, 1, 86, '2026-01-12 20:01:08.673833+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (450, 1, 86, '2026-01-12 20:16:08.762338+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (451, 1, 94, '2026-01-12 20:31:08.887233+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (452, 1, 83, '2026-01-12 20:46:09.092091+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (453, 1, 82, '2026-01-12 21:01:09.398161+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (454, 1, 86, '2026-01-12 21:16:09.484874+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (455, 1, 90, '2026-01-12 21:31:09.595203+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (456, 1, 70, '2026-01-12 21:46:09.902935+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (457, 1, 81, '2026-01-12 22:01:09.983751+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (458, 1, 89, '2026-01-12 22:16:10.065459+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (459, 1, 77, '2026-01-12 22:31:10.156079+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (460, 1, 94, '2026-01-12 22:46:10.317312+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (461, 1, 94, '2026-01-12 23:01:10.794621+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (462, 1, 91, '2026-01-12 23:16:13.007032+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (463, 1, 90, '2026-01-12 23:31:13.10092+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (464, 1, 88, '2026-01-12 23:46:13.620714+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (465, 1, 84, '2026-01-13 00:01:13.721163+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (466, 1, 68, '2026-01-14 14:10:47.457988+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (467, 1, 71, '2026-01-14 14:25:47.818503+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (468, 1, 73, '2026-01-14 14:40:47.913995+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (469, 1, 67, '2026-01-14 14:55:47.970141+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (470, 1, 68, '2026-01-14 15:10:48.408075+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (471, 1, 68, '2026-01-14 15:40:55.178858+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (472, 1, 72, '2026-01-14 15:55:55.770818+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (473, 1, 68, '2026-01-14 16:10:56.024585+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (474, 1, 61, '2026-01-14 16:25:56.28922+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (475, 1, 71, '2026-01-14 16:40:56.383848+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (476, 1, 72, '2026-01-14 16:55:56.751608+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (477, 1, 70, '2026-01-14 17:10:56.94593+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (478, 1, 70, '2026-01-14 17:25:57.128102+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (479, 1, 62, '2026-01-14 17:40:57.412257+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (480, 1, 77, '2026-01-14 17:55:57.505117+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (481, 1, 73, '2026-01-14 18:10:58.289519+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (482, 1, 66, '2026-01-14 18:25:58.389994+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (483, 1, 69, '2026-01-14 18:40:58.484442+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (484, 1, 60, '2026-01-14 18:55:58.805246+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (485, 1, 80, '2026-01-14 19:10:58.896803+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (486, 1, 74, '2026-01-14 19:25:59.423568+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (487, 1, 67, '2026-01-14 19:41:00.960836+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (488, 1, 63, '2026-01-14 19:56:01.06391+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (489, 1, 72, '2026-01-14 20:11:01.167148+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (490, 1, 71, '2026-01-14 20:26:01.482075+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (491, 1, 63, '2026-01-14 20:41:01.78487+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (492, 1, 67, '2026-01-14 20:56:02.061392+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (493, 1, 78, '2026-01-14 21:11:02.13242+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (494, 1, 60, '2026-01-14 21:26:02.311646+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (495, 1, 72, '2026-01-14 21:41:02.576991+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (496, 1, 64, '2026-01-14 21:56:02.955635+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (497, 1, 59, '2026-01-14 22:11:03.058204+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (498, 1, 71, '2026-01-14 22:26:03.160539+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (499, 1, 72, '2026-01-14 22:41:03.254936+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (500, 1, 77, '2026-01-14 22:56:03.397108+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (501, 1, 70, '2026-01-14 23:11:03.64585+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (502, 1, 75, '2026-01-14 23:26:04.063456+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (503, 1, 67, '2026-01-14 23:41:04.668407+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (504, 1, 72, '2026-01-14 23:56:04.991977+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (505, 1, 59, '2026-01-15 00:11:05.290953+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (506, 1, 71, '2026-01-15 00:26:05.572123+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (507, 1, 75, '2026-01-15 00:41:05.881095+01');
INSERT INTO public.sensor_data (id, plant_id, moisture_value, measured_at) VALUES (508, 1, 70, '2026-01-15 00:56:06.190308+01');


--
-- Name: devices_id_seq; Type: SEQUENCE SET; Schema: public; 
--

SELECT pg_catalog.setval('public.devices_id_seq', 2, true);


--
-- Name: groups_id_seq; Type: SEQUENCE SET; Schema: public; 
--

SELECT pg_catalog.setval('public.groups_id_seq', 1, true);


--
-- Name: plants_id_seq; Type: SEQUENCE SET; Schema: public; 
--

SELECT pg_catalog.setval('public.plants_id_seq', 2, true);


--
-- Name: sensor_data_id_seq; Type: SEQUENCE SET; Schema: public; 
--

SELECT pg_catalog.setval('public.sensor_data_id_seq', 508, true);


--
-- Name: devices devices_api_token_key; Type: CONSTRAINT; Schema: public; 
--

ALTER TABLE ONLY public.devices
    ADD CONSTRAINT devices_api_token_key UNIQUE (api_token);


--
-- Name: devices devices_mac_address_key; Type: CONSTRAINT; Schema: public; 
--

ALTER TABLE ONLY public.devices
    ADD CONSTRAINT devices_mac_address_key UNIQUE (mac_address);


--
-- Name: devices devices_pkey; Type: CONSTRAINT; Schema: public; 
--

ALTER TABLE ONLY public.devices
    ADD CONSTRAINT devices_pkey PRIMARY KEY (id);


--
-- Name: groups groups_pkey; Type: CONSTRAINT; Schema: public; 
--

ALTER TABLE ONLY public.groups
    ADD CONSTRAINT groups_pkey PRIMARY KEY (id);


--
-- Name: plants plants_pkey; Type: CONSTRAINT; Schema: public; 
--

ALTER TABLE ONLY public.plants
    ADD CONSTRAINT plants_pkey PRIMARY KEY (id);


--
-- Name: sensor_data sensor_data_pkey; Type: CONSTRAINT; Schema: public; 
--

ALTER TABLE ONLY public.sensor_data
    ADD CONSTRAINT sensor_data_pkey PRIMARY KEY (id);


--
-- Name: idx_devices_token; Type: INDEX; Schema: public; 
--

CREATE INDEX idx_devices_token ON public.devices USING btree (api_token);


--
-- Name: idx_sensor_plant_time; Type: INDEX; Schema: public; 
--

CREATE INDEX idx_sensor_plant_time ON public.sensor_data USING btree (plant_id, measured_at DESC);


--
-- Name: devices devices_current_plant_id_fkey; Type: FK CONSTRAINT; Schema: public; 
--

ALTER TABLE ONLY public.devices
    ADD CONSTRAINT devices_current_plant_id_fkey FOREIGN KEY (current_plant_id) REFERENCES public.plants(id) ON DELETE SET NULL;


--
-- Name: plants plants_group_id_fkey; Type: FK CONSTRAINT; Schema: public; 
--

ALTER TABLE ONLY public.plants
    ADD CONSTRAINT plants_group_id_fkey FOREIGN KEY (group_id) REFERENCES public.groups(id) ON DELETE SET NULL;


--
-- Name: sensor_data sensor_data_plant_id_fkey; Type: FK CONSTRAINT; Schema: public; 
--

ALTER TABLE ONLY public.sensor_data
    ADD CONSTRAINT sensor_data_plant_id_fkey FOREIGN KEY (plant_id) REFERENCES public.plants(id) ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--

\unrestrict iOYclZQjIyWyhO2b28gJTTfwiYaeR7KE34ACCQlgYXcdGm98h8jfXTwbdRWnVtj

