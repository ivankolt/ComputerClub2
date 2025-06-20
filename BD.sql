PGDMP           	            }            ComputerClub    15.8    16.3 �    V           0    0    ENCODING    ENCODING        SET client_encoding = 'UTF8';
                      false            W           0    0 
   STDSTRINGS 
   STDSTRINGS     (   SET standard_conforming_strings = 'on';
                      false            X           0    0 
   SEARCHPATH 
   SEARCHPATH     8   SELECT pg_catalog.set_config('search_path', '', false);
                      false            Y           1262    58692    ComputerClub    DATABASE     �   CREATE DATABASE "ComputerClub" WITH TEMPLATE = template0 ENCODING = 'UTF8' LOCALE_PROVIDER = libc LOCALE = 'Russian_Russia.1251';
    DROP DATABASE "ComputerClub";
                postgres    false                        2615    60043    audit    SCHEMA        CREATE SCHEMA audit;
    DROP SCHEMA audit;
                postgres    false            Z           0    0    SCHEMA audit    ACL     N   GRANT USAGE ON SCHEMA audit TO admin;
GRANT USAGE ON SCHEMA audit TO manager;
                   postgres    false    6            �           1247    59082    action_type_enum    TYPE     �   CREATE TYPE public.action_type_enum AS ENUM (
    'Вход',
    'Выход',
    'Покупка',
    'Бронирование'
);
 #   DROP TYPE public.action_type_enum;
       public          postgres    false            �           1247    59192    booking_status_enum    TYPE     �   CREATE TYPE public.booking_status_enum AS ENUM (
    'Ожидаемый',
    'Подтверждённый',
    'Завершённый',
    'Отменённый'
);
 &   DROP TYPE public.booking_status_enum;
       public          postgres    false            �           1247    59875    order_status_enum    TYPE     �   CREATE TYPE public.order_status_enum AS ENUM (
    'Ожидает обработки',
    'Собирается',
    'Готов к выдаче',
    'Отменён'
);
 $   DROP TYPE public.order_status_enum;
       public          postgres    false            �           1247    59846    payment_type_enum    TYPE     �   CREATE TYPE public.payment_type_enum AS ENUM (
    'покупка',
    'бронирование',
    'возврат',
    'отмена',
    'Пополнение средств на аккаунт'
);
 $   DROP TYPE public.payment_type_enum;
       public          postgres    false            �           1247    59168    pc_zone_enum    TYPE     t   CREATE TYPE public.pc_zone_enum AS ENUM (
    'Игровая',
    'VIP',
    'PlayStation',
    'Кокпит'
);
    DROP TYPE public.pc_zone_enum;
       public          postgres    false            �           1247    59014    position_name    TYPE     �   CREATE TYPE public.position_name AS ENUM (
    'Администратор',
    'Старший администратор',
    'Директор',
    'Менеджер'
);
     DROP TYPE public.position_name;
       public          postgres    false            �           1247    59895    product_type_enum    TYPE       CREATE TYPE public.product_type_enum AS ENUM (
    'БЕЗАЛКОГОЛЬНЫЕ_НАПИТКИ',
    'ЧИПСЫ_И_СНЕКИ',
    'ЭНЕРГЕТИЧЕСКИЕ_НАПИТКИ',
    'ИГРОВЫЕ_АКСЕССУАРЫ',
    'ШОКОЛАДНЫЕ_БАТОНЧИКИ'
);
 $   DROP TYPE public.product_type_enum;
       public          postgres    false            �           1247    59836    service_name_enum    TYPE     �   CREATE TYPE public.service_name_enum AS ENUM (
    'Бронирование ПК',
    'Товар',
    'Другое',
    'Пополнение средств на аккаунт'
);
 $   DROP TYPE public.service_name_enum;
       public          postgres    false            �           1247    58984 	   user_role    TYPE     ^   CREATE TYPE public.user_role AS ENUM (
    'user',
    'admin',
    'manager',
    'users'
);
    DROP TYPE public.user_role;
       public          postgres    false                       1255    60049    fn_log_changes()    FUNCTION     �  CREATE FUNCTION audit.fn_log_changes() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    INSERT INTO audit.payments_log(
        id, amount, type_payment, date_payment, service_name, account_number, user_id, _date, _user, _act
    ) 
    VALUES (
        NEW.id, NEW.amount, NEW.type_payment, NEW.date_payment, NEW.service_name, NEW.account_number, NEW.user_id, NOW(),
        CURRENT_USER, TG_OP
    );
    RETURN NULL;
END;
$$;
 &   DROP FUNCTION audit.fn_log_changes();
       audit          postgres    false    6                       1255    60074    archive_product(integer) 	   PROCEDURE     �   CREATE PROCEDURE public.archive_product(IN product_id integer)
    LANGUAGE plpgsql
    AS $$
BEGIN
    UPDATE products 
    SET deleted = true 
    WHERE id = product_id;
END;
$$;
 >   DROP PROCEDURE public.archive_product(IN product_id integer);
       public          postgres    false            [           0    0 0   PROCEDURE archive_product(IN product_id integer)    ACL     �   GRANT ALL ON PROCEDURE public.archive_product(IN product_id integer) TO admin;
GRANT ALL ON PROCEDURE public.archive_product(IN product_id integer) TO manager;
          public          postgres    false    271                        1255    59358    block_user(integer, text) 	   PROCEDURE     7  CREATE PROCEDURE public.block_user(IN user_id_param integer, IN block_reason text)
    LANGUAGE plpgsql
    AS $$
BEGIN
    INSERT INTO blocked_users (reason, block_date, user_id)
    VALUES (block_reason, CURRENT_DATE, user_id_param);

    UPDATE users SET is_active = FALSE WHERE id = user_id_param;
END;
$$;
 R   DROP PROCEDURE public.block_user(IN user_id_param integer, IN block_reason text);
       public          postgres    false                       1255    60052 S   book_pc(integer, integer, timestamp without time zone, timestamp without time zone)    FUNCTION     	  CREATE FUNCTION public.book_pc(p_pc_id integer, p_user_id integer, p_start_time timestamp without time zone, p_end_time timestamp without time zone) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
    v_booking_id integer;
    v_price_per_hour decimal;
    v_hours decimal;
    v_total_amount decimal;
    v_is_available boolean;
    v_user_balance decimal;
BEGIN
    -- Проверка доступности ПК
    SELECT check_pc_availability(p_pc_id, p_start_time, p_end_time)
    INTO v_is_available;

    IF NOT v_is_available THEN
        RAISE EXCEPTION 'ПК % недоступен в выбранное время', p_pc_id;
    END IF;

    -- Расчёт стоимости
    SELECT price_per_hour FROM pc WHERE id = p_pc_id INTO v_price_per_hour;
    v_hours := EXTRACT(EPOCH FROM (p_end_time - p_start_time)) / 3600;
    v_total_amount := v_price_per_hour * v_hours;

    -- Проверка баланса пользователя
    SELECT balance FROM users WHERE id = p_user_id INTO v_user_balance;

    IF v_user_balance < v_total_amount THEN
        -- Создание бронирования с статусом "Отменённый"
        INSERT INTO bookings (user_id, start_time, end_time, status, total_amount)
        VALUES (p_user_id, p_start_time, p_end_time, 'Отменённый', v_total_amount)
        RETURNING id INTO v_booking_id;

        -- Связывание с ПК
        INSERT INTO pc_bookings (booking_id, pc_id, qty_hour)
        VALUES (v_booking_id, p_pc_id, v_hours);

        RAISE EXCEPTION 'Недостаточно средств на счету';
    ELSE
        -- Создание бронирования с статусом "Подтверждённый"
        INSERT INTO bookings (user_id, start_time, end_time, status, total_amount)
        VALUES (p_user_id, p_start_time, p_end_time, 'Ожидаемый', v_total_amount)
        RETURNING id INTO v_booking_id;

        -- Связывание с ПК
        INSERT INTO pc_bookings (booking_id, pc_id, qty_hour)
        VALUES (v_booking_id, p_pc_id, v_hours);

        -- Списание средств
        UPDATE users SET balance = balance - v_total_amount WHERE id = p_user_id;
    END IF;

    RETURN v_booking_id;
EXCEPTION
    WHEN others THEN
        RAISE;
END;
$$;
 �   DROP FUNCTION public.book_pc(p_pc_id integer, p_user_id integer, p_start_time timestamp without time zone, p_end_time timestamp without time zone);
       public          postgres    false                       1255    59359 "   calculate_employee_salary(integer)    FUNCTION     �  CREATE FUNCTION public.calculate_employee_salary(employee_id_param integer) RETURNS numeric
    LANGUAGE plpgsql
    AS $$
DECLARE
    position_salary DECIMAL(10,2);
    total_day_hours DECIMAL(10,2);
    total_night_hours DECIMAL(10,2);
    night_bonus DECIMAL(10,2) := 0;
BEGIN
    SELECT salary INTO position_salary FROM posts 
    WHERE id = (SELECT position_id FROM employees WHERE id = employee_id_param);

    -- Часы дневных смен
    SELECT SUM(EXTRACT(EPOCH FROM (end_time - start_time)) / 3600) 
    INTO total_day_hours
    FROM shifts
    WHERE employee_id = employee_id_param
    AND start_time::TIME >= '08:00:00'::TIME
    AND end_time::TIME <= '20:00:00'::TIME
    AND end_time <= CURRENT_TIMESTAMP;

    -- Часы ночных смен
    SELECT SUM(EXTRACT(EPOCH FROM (end_time - start_time)) / 3600) 
    INTO total_night_hours
    FROM shifts
    WHERE employee_id = employee_id_param
    AND start_time::TIME >= '20:00:00'::TIME
    OR end_time::TIME <= '08:00:00'::TIME
    AND end_time <= CURRENT_TIMESTAMP;

    -- Надбавка за ночные часы
    night_bonus := total_night_hours * 500;

    RETURN position_salary + (total_day_hours * 100) + night_bonus;
END;
$$;
 K   DROP FUNCTION public.calculate_employee_salary(employee_id_param integer);
       public          postgres    false                       1255    60054 8   cancel_order_with_transaction(integer, integer, numeric) 	   PROCEDURE     ^  CREATE PROCEDURE public.cancel_order_with_transaction(IN order_id integer, IN user_id integer, IN amount numeric)
    LANGUAGE plpgsql
    AS $$
DECLARE
    check_order_result integer;
BEGIN
    SELECT COUNT(*) INTO check_order_result
    FROM orders
    WHERE id = order_id AND status = 'Ожидает обработки'::order_status_enum;

    IF check_order_result = 0 THEN
        RAISE EXCEPTION 'Заказ не найден или уже обработан';
    END IF;

    INSERT INTO payments (
        amount,
        type_payment,
        service_name,
        user_id,
        date_payment,
        account_number
    )
    VALUES (
        -amount,
        'возврат'::payment_type_enum,
        'Товар'::service_name_enum,
        user_id,
        NOW(),
        '2200665544778899'
    );

    UPDATE users
    SET balance = balance + amount
    WHERE id = user_id;

    UPDATE orders
    SET status = 'Отменён'::order_status_enum
    WHERE id = order_id;
EXCEPTION
    WHEN OTHERS THEN
        RAISE EXCEPTION 'Ошибка при отмене заказа: %', SQLERRM;
END;
$$;
 q   DROP PROCEDURE public.cancel_order_with_transaction(IN order_id integer, IN user_id integer, IN amount numeric);
       public          postgres    false                       1255    59356 X   check_pc_availability(integer, timestamp without time zone, timestamp without time zone)    FUNCTION     F  CREATE FUNCTION public.check_pc_availability(pc_id_param integer, start_time_param timestamp without time zone, end_time_param timestamp without time zone) RETURNS boolean
    LANGUAGE plpgsql
    AS $$
DECLARE
    is_available boolean;
BEGIN
    -- Проверка доступности компьютера
    SELECT NOT EXISTS (
        SELECT 1 
        FROM pc_bookings pb
        JOIN bookings b ON pb.booking_id = b.id
        WHERE pb.pc_id = pc_id_param
        AND b.status IN ('Ожидаемый', 'Подтверждённый')
        AND (
            (start_time_param BETWEEN b.start_time AND b.end_time) OR
            (end_time_param BETWEEN b.start_time AND b.end_time) OR
            (b.start_time BETWEEN start_time_param AND end_time_param)
        )
    ) INTO is_available;

    RETURN is_available;
END;
$$;
 �   DROP FUNCTION public.check_pc_availability(pc_id_param integer, start_time_param timestamp without time zone, end_time_param timestamp without time zone);
       public          postgres    false                       1255    60040 7   check_user_exists(character varying, character varying)    FUNCTION     �   CREATE FUNCTION public.check_user_exists(p_username character varying, p_email character varying) RETURNS smallint
    LANGUAGE plpgsql
    AS $$
BEGIN
    RETURN (SELECT COUNT(*) FROM Users WHERE username = p_username OR email = p_email);
END;
$$;
 a   DROP FUNCTION public.check_user_exists(p_username character varying, p_email character varying);
       public          postgres    false                       1255    60041 Z   create_booking(integer, timestamp without time zone, timestamp without time zone, numeric)    FUNCTION     �  CREATE FUNCTION public.create_booking(p_user_id integer, p_start_time timestamp without time zone, p_end_time timestamp without time zone, p_total_amount numeric) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
    booking_id INTEGER;
BEGIN
    INSERT INTO bookings (user_id, start_time, end_time, status, total_amount)
    VALUES (p_user_id, p_start_time, p_end_time, 'Ожидаемый', p_total_amount)
    RETURNING id INTO booking_id;
    
    RETURN booking_id;
END;
$$;
 �   DROP FUNCTION public.create_booking(p_user_id integer, p_start_time timestamp without time zone, p_end_time timestamp without time zone, p_total_amount numeric);
       public          postgres    false                       1255    59361    create_shifts_for_day(date)    FUNCTION     t  CREATE FUNCTION public.create_shifts_for_day(shift_date_param date) RETURNS void
    LANGUAGE plpgsql
    AS $$
DECLARE
    employee_rec RECORD;
    day_shift_start TIMESTAMP;
    day_shift_end TIMESTAMP;
    night_shift_start TIMESTAMP;
    night_shift_end TIMESTAMP;
BEGIN
    -- Создаем две смены: дневную (8:00-20:00) и ночную (20:00-8:00 следующего дня)
    FOR employee_rec IN SELECT * FROM employees LOOP
        -- Дневная смена
        day_shift_start := shift_date_param + '08:00:00'::TIME;
        day_shift_end := shift_date_param + '20:00:00'::TIME;
        INSERT INTO shifts (start_time, end_time, employee_id)
        VALUES (day_shift_start, day_shift_end, employee_rec.id);
        
        -- Ночная смена (продолжается до следующего дня)
        night_shift_start := shift_date_param + '20:00:00'::TIME;
        night_shift_end := night_shift_start + INTERVAL '12 hours';
        INSERT INTO shifts (start_time, end_time, employee_id)
        VALUES (night_shift_start, night_shift_end, employee_rec.id);
    END LOOP;
END;
$$;
 C   DROP FUNCTION public.create_shifts_for_day(shift_date_param date);
       public          postgres    false                       1255    59360 !   generate_sales_report(date, date)    FUNCTION     �  CREATE FUNCTION public.generate_sales_report(start_date_param date, end_date_param date) RETURNS TABLE(product_name character varying, total_sales numeric, sales_count integer)
    LANGUAGE plpgsql
    AS $$
BEGIN
    RETURN QUERY
    SELECT 
        p.product_name,
        SUM(op.quantity * p.price) AS total_sales,
        COUNT(op.order_id) AS sales_count
    FROM orders_products op
    JOIN products p ON op.product_id = p.id
    JOIN orders o ON op.order_id = o.id
    JOIN payments pay ON pay.type_payment = 'покупка'
    WHERE o.order_date BETWEEN start_date_param AND end_date_param
    GROUP BY p.product_name
    ORDER BY total_sales DESC;
END;
$$;
 X   DROP FUNCTION public.generate_sales_report(start_date_param date, end_date_param date);
       public          postgres    false                       1255    60038 F   insert_person(character varying, character varying, character varying)    FUNCTION     �  CREATE FUNCTION public.insert_person(p_first_name character varying, p_last_name character varying, p_phone_number character varying) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
    new_id INTEGER;
BEGIN
    INSERT INTO Persons (first_name, last_name, phone_number)
    VALUES (p_first_name, p_last_name, p_phone_number)
    RETURNING id INTO new_id;
    
    RETURN new_id;
END;
$$;
 �   DROP FUNCTION public.insert_person(p_first_name character varying, p_last_name character varying, p_phone_number character varying);
       public          postgres    false                       1255    76778 4   insert_user_action(public.action_type_enum, integer) 	   PROCEDURE     ,  CREATE PROCEDURE public.insert_user_action(IN p_action_type public.action_type_enum, IN p_user_id integer)
    LANGUAGE plpgsql
    AS $$
BEGIN
    INSERT INTO public.user_actions (action_type, action_time, user_id)
    VALUES (p_action_type, CURRENT_TIMESTAMP, p_user_id);
    
    COMMIT;
END;
$$;
 j   DROP PROCEDURE public.insert_user_action(IN p_action_type public.action_type_enum, IN p_user_id integer);
       public          postgres    false    922                       1255    60039 b   register_user(character varying, character varying, character varying, character varying, integer) 	   PROCEDURE     �  CREATE PROCEDURE public.register_user(IN p_username character varying, IN p_password_hash character varying, IN p_email character varying, IN p_card_number character varying, IN p_person_id integer, OUT p_success boolean, OUT p_message character varying)
    LANGUAGE plpgsql
    AS $$
begin
    if exists (select 1 from users where username = p_username) then
        p_success := false;
        p_message := 'Пользователь с таким логином уже существует';
        return;
    end if;
    
    if exists (select 1 from users where email = p_email) then
        p_success := false;
        p_message := 'Пользователь с таким email уже существует';
        return;
    end if;
    
    insert into users (username, password_hash, role, email, card_number, balance, is_active, person_id)
    values (p_username, p_password_hash, 'user', p_email, p_card_number, 0.00, false, p_person_id);
    
    p_success := true;
    p_message := 'Регистрация успешно завершена';
exception
    when others then
        p_success := false;
        p_message := 'Ошибка при регистрации: ' || sqlerrm;
end;
$$;
 �   DROP PROCEDURE public.register_user(IN p_username character varying, IN p_password_hash character varying, IN p_email character varying, IN p_card_number character varying, IN p_person_id integer, OUT p_success boolean, OUT p_message character varying);
       public          postgres    false                       1255    60075    restore_product(integer) 	   PROCEDURE     �   CREATE PROCEDURE public.restore_product(IN product_id integer)
    LANGUAGE plpgsql
    AS $$
BEGIN
    UPDATE products 
    SET deleted = false 
    WHERE id = product_id;
END;
$$;
 >   DROP PROCEDURE public.restore_product(IN product_id integer);
       public          postgres    false            \           0    0 0   PROCEDURE restore_product(IN product_id integer)    ACL     �   GRANT ALL ON PROCEDURE public.restore_product(IN product_id integer) TO admin;
GRANT ALL ON PROCEDURE public.restore_product(IN product_id integer) TO manager;
          public          postgres    false    264                       1255    59352    update_booking_end_time()    FUNCTION     �  CREATE FUNCTION public.update_booking_end_time() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
DECLARE
    total_hours DECIMAL(5, 2);
BEGIN
    SELECT COALESCE(SUM(pb.qty_hour), 0)
    INTO total_hours
    FROM pc_bookings pb
    WHERE pb.booking_id = NEW.booking_id;

    UPDATE bookings
    SET end_time = start_time + (total_hours || ' hours')::INTERVAL
    WHERE id = NEW.booking_id;

    RETURN NEW;
END;
$$;
 0   DROP FUNCTION public.update_booking_end_time();
       public          postgres    false                       1255    59357 :   update_booking_status(integer, public.booking_status_enum) 	   PROCEDURE     p  CREATE PROCEDURE public.update_booking_status(IN booking_id_param integer, IN new_status_param public.booking_status_enum)
    LANGUAGE plpgsql
    AS $$
BEGIN
    UPDATE bookings
    SET status = new_status_param
    WHERE id = booking_id_param;

    INSERT INTO user_actions (action_type, user_id)
    SELECT 
        CASE new_status_param
            WHEN 'Подтверждённый' THEN 'Бронирование'
            WHEN 'Отменённый' THEN 'Отмена'
            ELSE 'Изменение статуса'
        END,
        user_id
    FROM bookings
    WHERE id = booking_id_param;
END;
$$;
 z   DROP PROCEDURE public.update_booking_status(IN booking_id_param integer, IN new_status_param public.booking_status_enum);
       public          postgres    false    946                       1255    59354    update_booking_total_amount()    FUNCTION     w  CREATE FUNCTION public.update_booking_total_amount() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    UPDATE bookings
    SET total_amount = (
        SELECT SUM(pc.price_per_hour * pb.qty_hour)
        FROM pc_bookings pb
        JOIN pc ON pb.pc_id = pc.id
        WHERE pb.booking_id = NEW.booking_id
    )
    WHERE id = NEW.booking_id;

    RETURN NEW;
END;
$$;
 4   DROP FUNCTION public.update_booking_total_amount();
       public          postgres    false                       1255    59350    update_order_total_amount()    FUNCTION     r  CREATE FUNCTION public.update_order_total_amount() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    UPDATE orders
    SET total_amount = (
        SELECT SUM(op.quantity * p.price)
        FROM orders_products op
        JOIN products p ON op.product_id = p.id
        WHERE op.order_id = NEW.order_id
    )
    WHERE id = NEW.order_id;
    RETURN NEW;
END;
$$;
 2   DROP FUNCTION public.update_order_total_amount();
       public          postgres    false                       1255    59730    update_quantity_store()    FUNCTION     "  CREATE FUNCTION public.update_quantity_store() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    IF TG_OP = 'INSERT' THEN
        UPDATE products
        SET quantity_store = quantity_store + NEW.received_count
        WHERE id = NEW.products_id;
    ELSIF TG_OP = 'UPDATE' THEN
        IF OLD.products_id <> NEW.products_id THEN
            -- Удаляем старое количество из предыдущего продукта
            UPDATE products
            SET quantity_store = quantity_store - OLD.received_count
            WHERE id = OLD.products_id;
            
            -- Добавляем новое количество к новому продукту
            UPDATE products
            SET quantity_store = quantity_store + NEW.received_count
            WHERE id = NEW.products_id;
        ELSE
            UPDATE products
            SET quantity_store = quantity_store + (NEW.received_count - OLD.received_count)
            WHERE id = NEW.products_id;
        END IF;
    END IF;
    RETURN NEW;
END;
$$;
 .   DROP FUNCTION public.update_quantity_store();
       public          postgres    false            �            1259    60044    payments_log    TABLE     �  CREATE TABLE audit.payments_log (
    id integer,
    amount numeric(10,2),
    type_payment public.payment_type_enum,
    date_payment timestamp without time zone,
    service_name public.service_name_enum,
    account_number character varying(20),
    user_id integer,
    _date timestamp without time zone DEFAULT now(),
    _user character varying(50) DEFAULT CURRENT_USER,
    _act character varying(50)
);
    DROP TABLE audit.payments_log;
       audit         heap    postgres    false    6    964    961            ]           0    0    TABLE payments_log    ACL     �   GRANT SELECT,INSERT,DELETE,UPDATE ON TABLE audit.payments_log TO admin;
GRANT SELECT,INSERT,DELETE,UPDATE ON TABLE audit.payments_log TO manager;
          audit          postgres    false    249            �            1259    58992    users    TABLE     .  CREATE TABLE public.users (
    id integer NOT NULL,
    username character varying(50) NOT NULL,
    password_hash character varying(255) NOT NULL,
    role public.user_role NOT NULL,
    email character varying(100) NOT NULL,
    card_number character varying(16) NOT NULL,
    balance numeric(10,2) DEFAULT 0.00 NOT NULL,
    is_active boolean DEFAULT false NOT NULL,
    person_id integer NOT NULL,
    CONSTRAINT users_balance_check CHECK ((balance >= (0)::numeric)),
    CONSTRAINT users_card_number_check CHECK ((length((card_number)::text) = 16))
);
    DROP TABLE public.users;
       public         heap    postgres    false    901            ^           0    0    TABLE users    ACL     �   GRANT SELECT,INSERT,DELETE,UPDATE ON TABLE public.users TO admin;
GRANT SELECT,UPDATE ON TABLE public.users TO manager;
GRANT SELECT ON TABLE public.users TO users;
          public          postgres    false    218            _           0    0    COLUMN users.password_hash    ACL     <   GRANT UPDATE(password_hash) ON TABLE public.users TO users;
          public          postgres    false    218    3678            �            1259    68198    active_users    VIEW     �   CREATE VIEW public.active_users AS
 SELECT users.id,
    users.username,
    users.email,
    users.card_number,
    users.balance,
    users.is_active,
    users.person_id
   FROM public.users
  WHERE (users.role = 'users'::public.user_role);
    DROP VIEW public.active_users;
       public          postgres    false    218    901    218    218    218    218    218    218    218            �            1259    59067    blocked_users    TABLE     �   CREATE TABLE public.blocked_users (
    id integer NOT NULL,
    reason text NOT NULL,
    block_date date NOT NULL,
    user_id integer NOT NULL,
    CONSTRAINT blocked_users_block_date_check CHECK ((block_date <= CURRENT_DATE))
);
 !   DROP TABLE public.blocked_users;
       public         heap    postgres    false            �            1259    59066    blocked_users_id_seq    SEQUENCE     �   CREATE SEQUENCE public.blocked_users_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 +   DROP SEQUENCE public.blocked_users_id_seq;
       public          postgres    false    226            `           0    0    blocked_users_id_seq    SEQUENCE OWNED BY     M   ALTER SEQUENCE public.blocked_users_id_seq OWNED BY public.blocked_users.id;
          public          postgres    false    225            �            1259    59202    bookings    TABLE     }  CREATE TABLE public.bookings (
    id integer NOT NULL,
    start_time timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    end_time timestamp without time zone NOT NULL,
    status public.booking_status_enum NOT NULL,
    total_amount numeric(10,2),
    user_id integer NOT NULL,
    CONSTRAINT bookings_total_amount_check CHECK ((total_amount >= (0)::numeric))
);
    DROP TABLE public.bookings;
       public         heap    postgres    false    946            a           0    0    TABLE bookings    ACL     7   GRANT SELECT,UPDATE ON TABLE public.bookings TO admin;
          public          postgres    false    240            �            1259    59201    bookings_id_seq    SEQUENCE     �   CREATE SEQUENCE public.bookings_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 &   DROP SEQUENCE public.bookings_id_seq;
       public          postgres    false    240            b           0    0    bookings_id_seq    SEQUENCE OWNED BY     C   ALTER SEQUENCE public.bookings_id_seq OWNED BY public.bookings.id;
          public          postgres    false    239            �            1259    59034 	   employees    TABLE     L  CREATE TABLE public.employees (
    id integer NOT NULL,
    passport_series character varying(4),
    passport_number character varying(6),
    gender character(1),
    person_id integer NOT NULL,
    position_id integer NOT NULL,
    hire_date date NOT NULL,
    birthday date,
    CONSTRAINT employees_gender_check CHECK ((gender = ANY (ARRAY['М'::bpchar, 'Ж'::bpchar]))),
    CONSTRAINT employees_passport_number_check CHECK (((passport_number)::text ~* '^[0-9]{6}$'::text)),
    CONSTRAINT employees_passport_series_check CHECK (((passport_series)::text ~* '^[0-9]{4}$'::text))
);
    DROP TABLE public.employees;
       public         heap    postgres    false            c           0    0    TABLE employees    ACL     y   GRANT SELECT ON TABLE public.employees TO admin;
GRANT SELECT,INSERT,DELETE,UPDATE ON TABLE public.employees TO manager;
          public          postgres    false    222            �            1259    59033    employees_id_seq    SEQUENCE     �   CREATE SEQUENCE public.employees_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 '   DROP SEQUENCE public.employees_id_seq;
       public          postgres    false    222            d           0    0    employees_id_seq    SEQUENCE OWNED BY     E   ALTER SEQUENCE public.employees_id_seq OWNED BY public.employees.id;
          public          postgres    false    221            �            1259    59150 	   equipment    TABLE     ^  CREATE TABLE public.equipment (
    id integer NOT NULL,
    video_card character varying(255) NOT NULL,
    cpu character varying(255) NOT NULL,
    monitor character varying(255) NOT NULL,
    keyboard character varying(255) NOT NULL,
    monitor_hertz integer NOT NULL,
    CONSTRAINT equipment_monitor_hertz_check CHECK ((monitor_hertz >= 0))
);
    DROP TABLE public.equipment;
       public         heap    postgres    false            �            1259    59149    equipment_id_seq    SEQUENCE     �   CREATE SEQUENCE public.equipment_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 '   DROP SEQUENCE public.equipment_id_seq;
       public          postgres    false    236            e           0    0    equipment_id_seq    SEQUENCE OWNED BY     E   ALTER SEQUENCE public.equipment_id_seq OWNED BY public.equipment.id;
          public          postgres    false    235            �            1259    68190    equipment_info    VIEW     �   CREATE VIEW public.equipment_info AS
 SELECT equipment.id,
    equipment.video_card,
    equipment.cpu,
    equipment.monitor,
    equipment.keyboard,
    equipment.monitor_hertz
   FROM public.equipment
  ORDER BY equipment.id;
 !   DROP VIEW public.equipment_info;
       public          postgres    false    236    236    236    236    236    236            �            1259    59117    orders    TABLE     �  CREATE TABLE public.orders (
    id integer NOT NULL,
    order_date timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    total_amount numeric(10,2) NOT NULL,
    user_id integer NOT NULL,
    status public.order_status_enum DEFAULT 'Ожидает обработки'::public.order_status_enum NOT NULL,
    CONSTRAINT orders_order_date_check CHECK ((order_date <= CURRENT_TIMESTAMP)),
    CONSTRAINT orders_total_amount_check CHECK ((total_amount >= (0)::numeric))
);
    DROP TABLE public.orders;
       public         heap    postgres    false    970    970            f           0    0    TABLE orders    ACL     z   GRANT SELECT,UPDATE ON TABLE public.orders TO admin;
GRANT SELECT,INSERT,DELETE,UPDATE ON TABLE public.orders TO manager;
          public          postgres    false    232            �            1259    59132    orders_products    TABLE     �   CREATE TABLE public.orders_products (
    id integer NOT NULL,
    quantity integer NOT NULL,
    product_id integer NOT NULL,
    order_id integer NOT NULL,
    CONSTRAINT orders_products_quantity_check CHECK ((quantity >= 0))
);
 #   DROP TABLE public.orders_products;
       public         heap    postgres    false            �            1259    59106    products    TABLE       CREATE TABLE public.products (
    id integer NOT NULL,
    product_name character varying(255) NOT NULL,
    price numeric(10,2) NOT NULL,
    quantity_store integer NOT NULL,
    picture character varying(255) NOT NULL,
    product_type public.product_type_enum,
    deleted boolean DEFAULT false,
    CONSTRAINT products_price_check CHECK ((price >= (0)::numeric)),
    CONSTRAINT products_price_positive CHECK ((price >= (0)::numeric)),
    CONSTRAINT products_quantity_check CHECK ((quantity_store >= 0))
);
    DROP TABLE public.products;
       public         heap    postgres    false    973            g           0    0    TABLE products    ACL     �   GRANT SELECT ON TABLE public.products TO users;
GRANT SELECT,INSERT,DELETE,UPDATE ON TABLE public.products TO admin;
GRANT SELECT,INSERT,DELETE,UPDATE ON TABLE public.products TO manager;
          public          postgres    false    230            �            1259    68213    order_details_view    VIEW     �  CREATE VIEW public.order_details_view AS
 SELECT orders.id AS order_id,
    products.product_name AS product,
    orders_products.quantity,
    products.price AS price_per_unit,
    ((orders_products.quantity)::numeric * products.price) AS total_price
   FROM ((public.orders_products
     JOIN public.products ON ((orders_products.product_id = products.id)))
     JOIN public.orders ON ((orders_products.order_id = orders.id)));
 %   DROP VIEW public.order_details_view;
       public          postgres    false    230    230    230    232    234    234    234            �            1259    59116    orders_id_seq    SEQUENCE     �   CREATE SEQUENCE public.orders_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 $   DROP SEQUENCE public.orders_id_seq;
       public          postgres    false    232            h           0    0    orders_id_seq    SEQUENCE OWNED BY     ?   ALTER SEQUENCE public.orders_id_seq OWNED BY public.orders.id;
          public          postgres    false    231            �            1259    59131    orders_products_id_seq    SEQUENCE     �   CREATE SEQUENCE public.orders_products_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 -   DROP SEQUENCE public.orders_products_id_seq;
       public          postgres    false    234            i           0    0    orders_products_id_seq    SEQUENCE OWNED BY     Q   ALTER SEQUENCE public.orders_products_id_seq OWNED BY public.orders_products.id;
          public          postgres    false    233            �            1259    59834    payments_id_seq    SEQUENCE     x   CREATE SEQUENCE public.payments_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 &   DROP SEQUENCE public.payments_id_seq;
       public          postgres    false            j           0    0    SEQUENCE payments_id_seq    ACL     �   GRANT SELECT,USAGE ON SEQUENCE public.payments_id_seq TO admin;
GRANT SELECT,USAGE ON SEQUENCE public.payments_id_seq TO manager;
          public          postgres    false    247            �            1259    59857    payments    TABLE     �  CREATE TABLE public.payments (
    id integer DEFAULT nextval('public.payments_id_seq'::regclass) NOT NULL,
    amount numeric(10,2) NOT NULL,
    type_payment public.payment_type_enum NOT NULL,
    date_payment timestamp without time zone DEFAULT now() NOT NULL,
    service_name public.service_name_enum NOT NULL,
    account_number character varying(20) DEFAULT '2200665544778899'::character varying NOT NULL,
    user_id integer NOT NULL
);
    DROP TABLE public.payments;
       public         heap    postgres    false    247    964    961            k           0    0    TABLE payments    ACL     �   GRANT SELECT ON TABLE public.payments TO users;
GRANT ALL ON TABLE public.payments TO admin;
GRANT ALL ON TABLE public.payments TO manager;
          public          postgres    false    248            �            1259    59178    pc    TABLE     '  CREATE TABLE public.pc (
    id integer NOT NULL,
    price_per_hour numeric(10,2) NOT NULL,
    zone public.pc_zone_enum NOT NULL,
    activity boolean DEFAULT false NOT NULL,
    equipment_id integer NOT NULL,
    CONSTRAINT pc_price_per_hour_check CHECK ((price_per_hour >= (0)::numeric))
);
    DROP TABLE public.pc;
       public         heap    postgres    false    940            l           0    0    TABLE pc    ACL     �   GRANT SELECT ON TABLE public.pc TO users;
GRANT SELECT,INSERT,DELETE,UPDATE ON TABLE public.pc TO admin;
GRANT SELECT,INSERT,DELETE,UPDATE ON TABLE public.pc TO manager;
          public          postgres    false    238            �            1259    59217    pc_bookings    TABLE     �   CREATE TABLE public.pc_bookings (
    id integer NOT NULL,
    qty_hour numeric(5,2) NOT NULL,
    booking_id integer NOT NULL,
    pc_id integer NOT NULL,
    CONSTRAINT pc_bookings_qty_hour_check CHECK ((qty_hour >= 0.0))
);
    DROP TABLE public.pc_bookings;
       public         heap    postgres    false            �            1259    59216    pc_bookings_id_seq    SEQUENCE     �   CREATE SEQUENCE public.pc_bookings_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 )   DROP SEQUENCE public.pc_bookings_id_seq;
       public          postgres    false    242            m           0    0    pc_bookings_id_seq    SEQUENCE OWNED BY     I   ALTER SEQUENCE public.pc_bookings_id_seq OWNED BY public.pc_bookings.id;
          public          postgres    false    241            �            1259    59177 	   pc_id_seq    SEQUENCE     �   CREATE SEQUENCE public.pc_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
     DROP SEQUENCE public.pc_id_seq;
       public          postgres    false    238            n           0    0 	   pc_id_seq    SEQUENCE OWNED BY     7   ALTER SEQUENCE public.pc_id_seq OWNED BY public.pc.id;
          public          postgres    false    237            �            1259    58974    persons    TABLE     "  CREATE TABLE public.persons (
    id integer NOT NULL,
    first_name character varying(50) NOT NULL,
    last_name character varying(50) NOT NULL,
    phone_number character varying(16),
    CONSTRAINT persons_phone_number_check CHECK (((phone_number)::text ~* '^\+7[0-9]{10}$'::text))
);
    DROP TABLE public.persons;
       public         heap    postgres    false            �            1259    58973    persons_id_seq    SEQUENCE     �   CREATE SEQUENCE public.persons_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 %   DROP SEQUENCE public.persons_id_seq;
       public          postgres    false    216            o           0    0    persons_id_seq    SEQUENCE OWNED BY     A   ALTER SEQUENCE public.persons_id_seq OWNED BY public.persons.id;
          public          postgres    false    215            �            1259    59024    posts    TABLE     �   CREATE TABLE public.posts (
    id integer NOT NULL,
    position_name character varying(50) NOT NULL,
    salary numeric(10,2) NOT NULL,
    CONSTRAINT posts_salary_check CHECK ((salary >= (0)::numeric))
);
    DROP TABLE public.posts;
       public         heap    postgres    false            �            1259    59023    posts_id_seq    SEQUENCE     �   CREATE SEQUENCE public.posts_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 #   DROP SEQUENCE public.posts_id_seq;
       public          postgres    false    220            p           0    0    posts_id_seq    SEQUENCE OWNED BY     =   ALTER SEQUENCE public.posts_id_seq OWNED BY public.posts.id;
          public          postgres    false    219            �            1259    59105    products_id_seq    SEQUENCE     �   CREATE SEQUENCE public.products_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 &   DROP SEQUENCE public.products_id_seq;
       public          postgres    false    230            q           0    0    products_id_seq    SEQUENCE OWNED BY     C   ALTER SEQUENCE public.products_id_seq OWNED BY public.products.id;
          public          postgres    false    229            �            1259    59269    receipts    TABLE       CREATE TABLE public.receipts (
    id integer NOT NULL,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    payment jsonb NOT NULL,
    payment_id integer NOT NULL,
    CONSTRAINT receipts_created_at_check CHECK ((created_at <= CURRENT_TIMESTAMP))
);
    DROP TABLE public.receipts;
       public         heap    postgres    false            �            1259    59268    receipts_id_seq    SEQUENCE     �   CREATE SEQUENCE public.receipts_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 &   DROP SEQUENCE public.receipts_id_seq;
       public          postgres    false    244            r           0    0    receipts_id_seq    SEQUENCE OWNED BY     C   ALTER SEQUENCE public.receipts_id_seq OWNED BY public.receipts.id;
          public          postgres    false    243            �            1259    59716    received_products    TABLE     �  CREATE TABLE public.received_products (
    id integer NOT NULL,
    date_receipt timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    received_count integer NOT NULL,
    product_id integer NOT NULL,
    CONSTRAINT received_products_date_receipt_check CHECK ((date_receipt <= CURRENT_TIMESTAMP)),
    CONSTRAINT received_products_received_count_check CHECK ((received_count >= 0))
);
 %   DROP TABLE public.received_products;
       public         heap    postgres    false            �            1259    59715    received_products_id_seq    SEQUENCE     �   CREATE SEQUENCE public.received_products_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 /   DROP SEQUENCE public.received_products_id_seq;
       public          postgres    false    246            s           0    0    received_products_id_seq    SEQUENCE OWNED BY     U   ALTER SEQUENCE public.received_products_id_seq OWNED BY public.received_products.id;
          public          postgres    false    245            �            1259    59054    shifts    TABLE     �   CREATE TABLE public.shifts (
    id integer NOT NULL,
    start_time timestamp without time zone NOT NULL,
    end_time timestamp without time zone,
    employee_id integer NOT NULL,
    CONSTRAINT valid_shift_time CHECK ((end_time > start_time))
);
    DROP TABLE public.shifts;
       public         heap    postgres    false            �            1259    59053    shifts_id_seq    SEQUENCE     �   CREATE SEQUENCE public.shifts_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 $   DROP SEQUENCE public.shifts_id_seq;
       public          postgres    false    224            t           0    0    shifts_id_seq    SEQUENCE OWNED BY     ?   ALTER SEQUENCE public.shifts_id_seq OWNED BY public.shifts.id;
          public          postgres    false    223            �            1259    59092    user_actions    TABLE     �   CREATE TABLE public.user_actions (
    id integer NOT NULL,
    action_type public.action_type_enum NOT NULL,
    action_time timestamp without time zone DEFAULT CURRENT_DATE NOT NULL,
    user_id integer NOT NULL
);
     DROP TABLE public.user_actions;
       public         heap    postgres    false    922            �            1259    59091    user_actions_id_seq    SEQUENCE     �   CREATE SEQUENCE public.user_actions_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 *   DROP SEQUENCE public.user_actions_id_seq;
       public          postgres    false    228            u           0    0    user_actions_id_seq    SEQUENCE OWNED BY     K   ALTER SEQUENCE public.user_actions_id_seq OWNED BY public.user_actions.id;
          public          postgres    false    227            �            1259    68202    user_details_with_block_status    VIEW     �  CREATE VIEW public.user_details_with_block_status AS
 SELECT p.first_name,
    p.last_name,
    p.phone_number,
    u.username,
    u.email,
    u.card_number,
    u.balance,
        CASE
            WHEN (bu.id IS NULL) THEN false
            ELSE true
        END AS is_blocked
   FROM ((public.users u
     JOIN public.persons p ON ((u.person_id = p.id)))
     LEFT JOIN public.blocked_users bu ON ((u.id = bu.user_id)));
 1   DROP VIEW public.user_details_with_block_status;
       public          postgres    false    216    216    216    216    218    218    218    218    218    218    226    226            �            1259    68208    user_full_info    VIEW     �  CREATE VIEW public.user_full_info AS
 SELECT u.id AS user_id,
    p.first_name,
    p.last_name,
    p.phone_number,
    u.email,
    u.username,
    u.card_number,
    u.balance,
        CASE
            WHEN (bu.id IS NULL) THEN false
            ELSE true
        END AS is_blocked
   FROM ((public.users u
     JOIN public.persons p ON ((u.person_id = p.id)))
     LEFT JOIN public.blocked_users bu ON ((u.id = bu.user_id)));
 !   DROP VIEW public.user_full_info;
       public          postgres    false    218    218    218    218    218    218    216    216    226    226    216    216            �            1259    68194    user_personal_info    VIEW       CREATE VIEW public.user_personal_info AS
 SELECT persons.first_name,
    persons.last_name,
    persons.phone_number,
    users.email,
    users.username,
    users.card_number
   FROM (public.users
     JOIN public.persons ON ((users.person_id = persons.id)));
 %   DROP VIEW public.user_personal_info;
       public          postgres    false    216    216    218    218    216    218    216    218            �            1259    58991    users_id_seq    SEQUENCE     �   CREATE SEQUENCE public.users_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 #   DROP SEQUENCE public.users_id_seq;
       public          postgres    false    218            v           0    0    users_id_seq    SEQUENCE OWNED BY     =   ALTER SEQUENCE public.users_id_seq OWNED BY public.users.id;
          public          postgres    false    217                       2604    59070    blocked_users id    DEFAULT     t   ALTER TABLE ONLY public.blocked_users ALTER COLUMN id SET DEFAULT nextval('public.blocked_users_id_seq'::regclass);
 ?   ALTER TABLE public.blocked_users ALTER COLUMN id DROP DEFAULT;
       public          postgres    false    225    226    226                       2604    59205    bookings id    DEFAULT     j   ALTER TABLE ONLY public.bookings ALTER COLUMN id SET DEFAULT nextval('public.bookings_id_seq'::regclass);
 :   ALTER TABLE public.bookings ALTER COLUMN id DROP DEFAULT;
       public          postgres    false    239    240    240                       2604    59037    employees id    DEFAULT     l   ALTER TABLE ONLY public.employees ALTER COLUMN id SET DEFAULT nextval('public.employees_id_seq'::regclass);
 ;   ALTER TABLE public.employees ALTER COLUMN id DROP DEFAULT;
       public          postgres    false    221    222    222                       2604    59153    equipment id    DEFAULT     l   ALTER TABLE ONLY public.equipment ALTER COLUMN id SET DEFAULT nextval('public.equipment_id_seq'::regclass);
 ;   ALTER TABLE public.equipment ALTER COLUMN id DROP DEFAULT;
       public          postgres    false    236    235    236                       2604    59120 	   orders id    DEFAULT     f   ALTER TABLE ONLY public.orders ALTER COLUMN id SET DEFAULT nextval('public.orders_id_seq'::regclass);
 8   ALTER TABLE public.orders ALTER COLUMN id DROP DEFAULT;
       public          postgres    false    232    231    232                       2604    59135    orders_products id    DEFAULT     x   ALTER TABLE ONLY public.orders_products ALTER COLUMN id SET DEFAULT nextval('public.orders_products_id_seq'::regclass);
 A   ALTER TABLE public.orders_products ALTER COLUMN id DROP DEFAULT;
       public          postgres    false    233    234    234                       2604    59181    pc id    DEFAULT     ^   ALTER TABLE ONLY public.pc ALTER COLUMN id SET DEFAULT nextval('public.pc_id_seq'::regclass);
 4   ALTER TABLE public.pc ALTER COLUMN id DROP DEFAULT;
       public          postgres    false    238    237    238                       2604    59220    pc_bookings id    DEFAULT     p   ALTER TABLE ONLY public.pc_bookings ALTER COLUMN id SET DEFAULT nextval('public.pc_bookings_id_seq'::regclass);
 =   ALTER TABLE public.pc_bookings ALTER COLUMN id DROP DEFAULT;
       public          postgres    false    242    241    242                       2604    58977 
   persons id    DEFAULT     h   ALTER TABLE ONLY public.persons ALTER COLUMN id SET DEFAULT nextval('public.persons_id_seq'::regclass);
 9   ALTER TABLE public.persons ALTER COLUMN id DROP DEFAULT;
       public          postgres    false    216    215    216                       2604    59027    posts id    DEFAULT     d   ALTER TABLE ONLY public.posts ALTER COLUMN id SET DEFAULT nextval('public.posts_id_seq'::regclass);
 7   ALTER TABLE public.posts ALTER COLUMN id DROP DEFAULT;
       public          postgres    false    219    220    220                       2604    59109    products id    DEFAULT     j   ALTER TABLE ONLY public.products ALTER COLUMN id SET DEFAULT nextval('public.products_id_seq'::regclass);
 :   ALTER TABLE public.products ALTER COLUMN id DROP DEFAULT;
       public          postgres    false    230    229    230                       2604    59272    receipts id    DEFAULT     j   ALTER TABLE ONLY public.receipts ALTER COLUMN id SET DEFAULT nextval('public.receipts_id_seq'::regclass);
 :   ALTER TABLE public.receipts ALTER COLUMN id DROP DEFAULT;
       public          postgres    false    243    244    244                       2604    59719    received_products id    DEFAULT     |   ALTER TABLE ONLY public.received_products ALTER COLUMN id SET DEFAULT nextval('public.received_products_id_seq'::regclass);
 C   ALTER TABLE public.received_products ALTER COLUMN id DROP DEFAULT;
       public          postgres    false    246    245    246                       2604    59057 	   shifts id    DEFAULT     f   ALTER TABLE ONLY public.shifts ALTER COLUMN id SET DEFAULT nextval('public.shifts_id_seq'::regclass);
 8   ALTER TABLE public.shifts ALTER COLUMN id DROP DEFAULT;
       public          postgres    false    224    223    224            	           2604    59095    user_actions id    DEFAULT     r   ALTER TABLE ONLY public.user_actions ALTER COLUMN id SET DEFAULT nextval('public.user_actions_id_seq'::regclass);
 >   ALTER TABLE public.user_actions ALTER COLUMN id DROP DEFAULT;
       public          postgres    false    227    228    228                       2604    58995    users id    DEFAULT     d   ALTER TABLE ONLY public.users ALTER COLUMN id SET DEFAULT nextval('public.users_id_seq'::regclass);
 7   ALTER TABLE public.users ALTER COLUMN id DROP DEFAULT;
       public          postgres    false    217    218    218            S          0    60044    payments_log 
   TABLE DATA           �   COPY audit.payments_log (id, amount, type_payment, date_payment, service_name, account_number, user_id, _date, _user, _act) FROM stdin;
    audit          postgres    false    249   P      <          0    59067    blocked_users 
   TABLE DATA           H   COPY public.blocked_users (id, reason, block_date, user_id) FROM stdin;
    public          postgres    false    226   �S      J          0    59202    bookings 
   TABLE DATA           [   COPY public.bookings (id, start_time, end_time, status, total_amount, user_id) FROM stdin;
    public          postgres    false    240   T      8          0    59034 	   employees 
   TABLE DATA           ~   COPY public.employees (id, passport_series, passport_number, gender, person_id, position_id, hire_date, birthday) FROM stdin;
    public          postgres    false    222   �X      F          0    59150 	   equipment 
   TABLE DATA           Z   COPY public.equipment (id, video_card, cpu, monitor, keyboard, monitor_hertz) FROM stdin;
    public          postgres    false    236   �X      B          0    59117    orders 
   TABLE DATA           O   COPY public.orders (id, order_date, total_amount, user_id, status) FROM stdin;
    public          postgres    false    232   Z      D          0    59132    orders_products 
   TABLE DATA           M   COPY public.orders_products (id, quantity, product_id, order_id) FROM stdin;
    public          postgres    false    234   �]      R          0    59857    payments 
   TABLE DATA           q   COPY public.payments (id, amount, type_payment, date_payment, service_name, account_number, user_id) FROM stdin;
    public          postgres    false    248   �_      H          0    59178    pc 
   TABLE DATA           N   COPY public.pc (id, price_per_hour, zone, activity, equipment_id) FROM stdin;
    public          postgres    false    238   "f      L          0    59217    pc_bookings 
   TABLE DATA           F   COPY public.pc_bookings (id, qty_hour, booking_id, pc_id) FROM stdin;
    public          postgres    false    242   g      2          0    58974    persons 
   TABLE DATA           J   COPY public.persons (id, first_name, last_name, phone_number) FROM stdin;
    public          postgres    false    216   jh      6          0    59024    posts 
   TABLE DATA           :   COPY public.posts (id, position_name, salary) FROM stdin;
    public          postgres    false    220   �h      @          0    59106    products 
   TABLE DATA           k   COPY public.products (id, product_name, price, quantity_store, picture, product_type, deleted) FROM stdin;
    public          postgres    false    230   7i      N          0    59269    receipts 
   TABLE DATA           G   COPY public.receipts (id, created_at, payment, payment_id) FROM stdin;
    public          postgres    false    244   l      P          0    59716    received_products 
   TABLE DATA           Y   COPY public.received_products (id, date_receipt, received_count, product_id) FROM stdin;
    public          postgres    false    246   �t      :          0    59054    shifts 
   TABLE DATA           G   COPY public.shifts (id, start_time, end_time, employee_id) FROM stdin;
    public          postgres    false    224   Pu      >          0    59092    user_actions 
   TABLE DATA           M   COPY public.user_actions (id, action_type, action_time, user_id) FROM stdin;
    public          postgres    false    228   3v      4          0    58992    users 
   TABLE DATA           u   COPY public.users (id, username, password_hash, role, email, card_number, balance, is_active, person_id) FROM stdin;
    public          postgres    false    218   Mx      w           0    0    blocked_users_id_seq    SEQUENCE SET     B   SELECT pg_catalog.setval('public.blocked_users_id_seq', 3, true);
          public          postgres    false    225            x           0    0    bookings_id_seq    SEQUENCE SET     >   SELECT pg_catalog.setval('public.bookings_id_seq', 98, true);
          public          postgres    false    239            y           0    0    employees_id_seq    SEQUENCE SET     >   SELECT pg_catalog.setval('public.employees_id_seq', 3, true);
          public          postgres    false    221            z           0    0    equipment_id_seq    SEQUENCE SET     >   SELECT pg_catalog.setval('public.equipment_id_seq', 5, true);
          public          postgres    false    235            {           0    0    orders_id_seq    SEQUENCE SET     <   SELECT pg_catalog.setval('public.orders_id_seq', 55, true);
          public          postgres    false    231            |           0    0    orders_products_id_seq    SEQUENCE SET     E   SELECT pg_catalog.setval('public.orders_products_id_seq', 99, true);
          public          postgres    false    233            }           0    0    payments_id_seq    SEQUENCE SET     ?   SELECT pg_catalog.setval('public.payments_id_seq', 146, true);
          public          postgres    false    247            ~           0    0    pc_bookings_id_seq    SEQUENCE SET     A   SELECT pg_catalog.setval('public.pc_bookings_id_seq', 84, true);
          public          postgres    false    241                       0    0 	   pc_id_seq    SEQUENCE SET     8   SELECT pg_catalog.setval('public.pc_id_seq', 41, true);
          public          postgres    false    237            �           0    0    persons_id_seq    SEQUENCE SET     =   SELECT pg_catalog.setval('public.persons_id_seq', 15, true);
          public          postgres    false    215            �           0    0    posts_id_seq    SEQUENCE SET     ;   SELECT pg_catalog.setval('public.posts_id_seq', 1, false);
          public          postgres    false    219            �           0    0    products_id_seq    SEQUENCE SET     >   SELECT pg_catalog.setval('public.products_id_seq', 20, true);
          public          postgres    false    229            �           0    0    receipts_id_seq    SEQUENCE SET     >   SELECT pg_catalog.setval('public.receipts_id_seq', 31, true);
          public          postgres    false    243            �           0    0    received_products_id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('public.received_products_id_seq', 5, true);
          public          postgres    false    245            �           0    0    shifts_id_seq    SEQUENCE SET     <   SELECT pg_catalog.setval('public.shifts_id_seq', 13, true);
          public          postgres    false    223            �           0    0    user_actions_id_seq    SEQUENCE SET     B   SELECT pg_catalog.setval('public.user_actions_id_seq', 82, true);
          public          postgres    false    227            �           0    0    users_id_seq    SEQUENCE SET     :   SELECT pg_catalog.setval('public.users_id_seq', 7, true);
          public          postgres    false    217            P           2606    59075     blocked_users blocked_users_pkey 
   CONSTRAINT     ^   ALTER TABLE ONLY public.blocked_users
    ADD CONSTRAINT blocked_users_pkey PRIMARY KEY (id);
 J   ALTER TABLE ONLY public.blocked_users DROP CONSTRAINT blocked_users_pkey;
       public            postgres    false    226            t           2606    59210    bookings bookings_pkey 
   CONSTRAINT     T   ALTER TABLE ONLY public.bookings
    ADD CONSTRAINT bookings_pkey PRIMARY KEY (id);
 @   ALTER TABLE ONLY public.bookings DROP CONSTRAINT bookings_pkey;
       public            postgres    false    240            I           2606    59042    employees employees_pkey 
   CONSTRAINT     V   ALTER TABLE ONLY public.employees
    ADD CONSTRAINT employees_pkey PRIMARY KEY (id);
 B   ALTER TABLE ONLY public.employees DROP CONSTRAINT employees_pkey;
       public            postgres    false    222            h           2606    59162    equipment equipment_cpu_key 
   CONSTRAINT     U   ALTER TABLE ONLY public.equipment
    ADD CONSTRAINT equipment_cpu_key UNIQUE (cpu);
 E   ALTER TABLE ONLY public.equipment DROP CONSTRAINT equipment_cpu_key;
       public            postgres    false    236            j           2606    59166     equipment equipment_keyboard_key 
   CONSTRAINT     _   ALTER TABLE ONLY public.equipment
    ADD CONSTRAINT equipment_keyboard_key UNIQUE (keyboard);
 J   ALTER TABLE ONLY public.equipment DROP CONSTRAINT equipment_keyboard_key;
       public            postgres    false    236            l           2606    59164    equipment equipment_monitor_key 
   CONSTRAINT     ]   ALTER TABLE ONLY public.equipment
    ADD CONSTRAINT equipment_monitor_key UNIQUE (monitor);
 I   ALTER TABLE ONLY public.equipment DROP CONSTRAINT equipment_monitor_key;
       public            postgres    false    236            n           2606    59158    equipment equipment_pkey 
   CONSTRAINT     V   ALTER TABLE ONLY public.equipment
    ADD CONSTRAINT equipment_pkey PRIMARY KEY (id);
 B   ALTER TABLE ONLY public.equipment DROP CONSTRAINT equipment_pkey;
       public            postgres    false    236            p           2606    59160 "   equipment equipment_video_card_key 
   CONSTRAINT     c   ALTER TABLE ONLY public.equipment
    ADD CONSTRAINT equipment_video_card_key UNIQUE (video_card);
 L   ALTER TABLE ONLY public.equipment DROP CONSTRAINT equipment_video_card_key;
       public            postgres    false    236            b           2606    59125    orders orders_pkey 
   CONSTRAINT     P   ALTER TABLE ONLY public.orders
    ADD CONSTRAINT orders_pkey PRIMARY KEY (id);
 <   ALTER TABLE ONLY public.orders DROP CONSTRAINT orders_pkey;
       public            postgres    false    232            f           2606    59138 $   orders_products orders_products_pkey 
   CONSTRAINT     b   ALTER TABLE ONLY public.orders_products
    ADD CONSTRAINT orders_products_pkey PRIMARY KEY (id);
 N   ALTER TABLE ONLY public.orders_products DROP CONSTRAINT orders_products_pkey;
       public            postgres    false    234            �           2606    59865    payments payments_pkey 
   CONSTRAINT     T   ALTER TABLE ONLY public.payments
    ADD CONSTRAINT payments_pkey PRIMARY KEY (id);
 @   ALTER TABLE ONLY public.payments DROP CONSTRAINT payments_pkey;
       public            postgres    false    248            |           2606    59223    pc_bookings pc_bookings_pkey 
   CONSTRAINT     Z   ALTER TABLE ONLY public.pc_bookings
    ADD CONSTRAINT pc_bookings_pkey PRIMARY KEY (id);
 F   ALTER TABLE ONLY public.pc_bookings DROP CONSTRAINT pc_bookings_pkey;
       public            postgres    false    242            r           2606    59185 
   pc pc_pkey 
   CONSTRAINT     H   ALTER TABLE ONLY public.pc
    ADD CONSTRAINT pc_pkey PRIMARY KEY (id);
 4   ALTER TABLE ONLY public.pc DROP CONSTRAINT pc_pkey;
       public            postgres    false    238            7           2606    58982     persons persons_phone_number_key 
   CONSTRAINT     c   ALTER TABLE ONLY public.persons
    ADD CONSTRAINT persons_phone_number_key UNIQUE (phone_number);
 J   ALTER TABLE ONLY public.persons DROP CONSTRAINT persons_phone_number_key;
       public            postgres    false    216            9           2606    58980    persons persons_pkey 
   CONSTRAINT     R   ALTER TABLE ONLY public.persons
    ADD CONSTRAINT persons_pkey PRIMARY KEY (id);
 >   ALTER TABLE ONLY public.persons DROP CONSTRAINT persons_pkey;
       public            postgres    false    216            E           2606    59030    posts posts_pkey 
   CONSTRAINT     N   ALTER TABLE ONLY public.posts
    ADD CONSTRAINT posts_pkey PRIMARY KEY (id);
 :   ALTER TABLE ONLY public.posts DROP CONSTRAINT posts_pkey;
       public            postgres    false    220            G           2606    59032    posts posts_position_name_key 
   CONSTRAINT     a   ALTER TABLE ONLY public.posts
    ADD CONSTRAINT posts_position_name_key UNIQUE (position_name);
 G   ALTER TABLE ONLY public.posts DROP CONSTRAINT posts_position_name_key;
       public            postgres    false    220            Y           2606    59710    products products_picture_key 
   CONSTRAINT     [   ALTER TABLE ONLY public.products
    ADD CONSTRAINT products_picture_key UNIQUE (picture);
 G   ALTER TABLE ONLY public.products DROP CONSTRAINT products_picture_key;
       public            postgres    false    230            [           2606    59113    products products_pkey 
   CONSTRAINT     T   ALTER TABLE ONLY public.products
    ADD CONSTRAINT products_pkey PRIMARY KEY (id);
 @   ALTER TABLE ONLY public.products DROP CONSTRAINT products_pkey;
       public            postgres    false    230            ]           2606    59115 "   products products_product_name_key 
   CONSTRAINT     e   ALTER TABLE ONLY public.products
    ADD CONSTRAINT products_product_name_key UNIQUE (product_name);
 L   ALTER TABLE ONLY public.products DROP CONSTRAINT products_product_name_key;
       public            postgres    false    230            �           2606    59278    receipts receipts_pkey 
   CONSTRAINT     T   ALTER TABLE ONLY public.receipts
    ADD CONSTRAINT receipts_pkey PRIMARY KEY (id);
 @   ALTER TABLE ONLY public.receipts DROP CONSTRAINT receipts_pkey;
       public            postgres    false    244            �           2606    59724 (   received_products received_products_pkey 
   CONSTRAINT     f   ALTER TABLE ONLY public.received_products
    ADD CONSTRAINT received_products_pkey PRIMARY KEY (id);
 R   ALTER TABLE ONLY public.received_products DROP CONSTRAINT received_products_pkey;
       public            postgres    false    246            N           2606    59060    shifts shifts_pkey 
   CONSTRAINT     P   ALTER TABLE ONLY public.shifts
    ADD CONSTRAINT shifts_pkey PRIMARY KEY (id);
 <   ALTER TABLE ONLY public.shifts DROP CONSTRAINT shifts_pkey;
       public            postgres    false    224            U           2606    59099    user_actions user_actions_pkey 
   CONSTRAINT     \   ALTER TABLE ONLY public.user_actions
    ADD CONSTRAINT user_actions_pkey PRIMARY KEY (id);
 H   ALTER TABLE ONLY public.user_actions DROP CONSTRAINT user_actions_pkey;
       public            postgres    false    228            =           2606    59007    users users_card_number_key 
   CONSTRAINT     ]   ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_card_number_key UNIQUE (card_number);
 E   ALTER TABLE ONLY public.users DROP CONSTRAINT users_card_number_key;
       public            postgres    false    218            ?           2606    59005    users users_email_key 
   CONSTRAINT     Q   ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_email_key UNIQUE (email);
 ?   ALTER TABLE ONLY public.users DROP CONSTRAINT users_email_key;
       public            postgres    false    218            A           2606    59001    users users_pkey 
   CONSTRAINT     N   ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (id);
 :   ALTER TABLE ONLY public.users DROP CONSTRAINT users_pkey;
       public            postgres    false    218            C           2606    59003    users users_username_key 
   CONSTRAINT     W   ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_username_key UNIQUE (username);
 B   ALTER TABLE ONLY public.users DROP CONSTRAINT users_username_key;
       public            postgres    false    218            Q           1259    59344    idx_blocked_users_user_id    INDEX     V   CREATE INDEX idx_blocked_users_user_id ON public.blocked_users USING btree (user_id);
 -   DROP INDEX public.idx_blocked_users_user_id;
       public            postgres    false    226            u           1259    59334    idx_bookings_end    INDEX     I   CREATE INDEX idx_bookings_end ON public.bookings USING btree (end_time);
 $   DROP INDEX public.idx_bookings_end;
       public            postgres    false    240            v           1259    59333    idx_bookings_start    INDEX     M   CREATE INDEX idx_bookings_start ON public.bookings USING btree (start_time);
 &   DROP INDEX public.idx_bookings_start;
       public            postgres    false    240            w           1259    59332    idx_bookings_status    INDEX     J   CREATE INDEX idx_bookings_status ON public.bookings USING btree (status);
 '   DROP INDEX public.idx_bookings_status;
       public            postgres    false    240            x           1259    59331    idx_bookings_user_id    INDEX     L   CREATE INDEX idx_bookings_user_id ON public.bookings USING btree (user_id);
 (   DROP INDEX public.idx_bookings_user_id;
       public            postgres    false    240            ^           1259    59336    idx_orders_date    INDEX     H   CREATE INDEX idx_orders_date ON public.orders USING btree (order_date);
 #   DROP INDEX public.idx_orders_date;
       public            postgres    false    232            c           1259    59342    idx_orders_products_order    INDEX     Y   CREATE INDEX idx_orders_products_order ON public.orders_products USING btree (order_id);
 -   DROP INDEX public.idx_orders_products_order;
       public            postgres    false    234            d           1259    59343    idx_orders_products_product    INDEX     ]   CREATE INDEX idx_orders_products_product ON public.orders_products USING btree (product_id);
 /   DROP INDEX public.idx_orders_products_product;
       public            postgres    false    234            _           1259    59337    idx_orders_total    INDEX     K   CREATE INDEX idx_orders_total ON public.orders USING btree (total_amount);
 $   DROP INDEX public.idx_orders_total;
       public            postgres    false    232            `           1259    59335    idx_orders_user_id    INDEX     H   CREATE INDEX idx_orders_user_id ON public.orders USING btree (user_id);
 &   DROP INDEX public.idx_orders_user_id;
       public            postgres    false    232            �           1259    59871    idx_payments_service    INDEX     Q   CREATE INDEX idx_payments_service ON public.payments USING btree (service_name);
 (   DROP INDEX public.idx_payments_service;
       public            postgres    false    248            �           1259    59872    idx_payments_type    INDEX     N   CREATE INDEX idx_payments_type ON public.payments USING btree (type_payment);
 %   DROP INDEX public.idx_payments_type;
       public            postgres    false    248            �           1259    59873    idx_payments_user_id    INDEX     L   CREATE INDEX idx_payments_user_id ON public.payments USING btree (user_id);
 (   DROP INDEX public.idx_payments_user_id;
       public            postgres    false    248            y           1259    59338    idx_pc_bookings_booking_id    INDEX     X   CREATE INDEX idx_pc_bookings_booking_id ON public.pc_bookings USING btree (booking_id);
 .   DROP INDEX public.idx_pc_bookings_booking_id;
       public            postgres    false    242            z           1259    59339    idx_pc_bookings_pc_id    INDEX     N   CREATE INDEX idx_pc_bookings_pc_id ON public.pc_bookings USING btree (pc_id);
 )   DROP INDEX public.idx_pc_bookings_pc_id;
       public            postgres    false    242            V           1259    59340    idx_products_name    INDEX     N   CREATE INDEX idx_products_name ON public.products USING btree (product_name);
 %   DROP INDEX public.idx_products_name;
       public            postgres    false    230            W           1259    59341    idx_products_price    INDEX     H   CREATE INDEX idx_products_price ON public.products USING btree (price);
 &   DROP INDEX public.idx_products_price;
       public            postgres    false    230            }           1259    59330    idx_receipts_created_at    INDEX     R   CREATE INDEX idx_receipts_created_at ON public.receipts USING btree (created_at);
 +   DROP INDEX public.idx_receipts_created_at;
       public            postgres    false    244            ~           1259    59329    idx_receipts_payment_id    INDEX     R   CREATE INDEX idx_receipts_payment_id ON public.receipts USING btree (payment_id);
 +   DROP INDEX public.idx_receipts_payment_id;
       public            postgres    false    244            J           1259    59345    idx_shifts_employee_id    INDEX     P   CREATE INDEX idx_shifts_employee_id ON public.shifts USING btree (employee_id);
 *   DROP INDEX public.idx_shifts_employee_id;
       public            postgres    false    224            K           1259    59347    idx_shifts_end    INDEX     E   CREATE INDEX idx_shifts_end ON public.shifts USING btree (end_time);
 "   DROP INDEX public.idx_shifts_end;
       public            postgres    false    224            L           1259    59346    idx_shifts_start    INDEX     I   CREATE INDEX idx_shifts_start ON public.shifts USING btree (start_time);
 $   DROP INDEX public.idx_shifts_start;
       public            postgres    false    224            R           1259    76780    idx_user_actions_time    INDEX     U   CREATE INDEX idx_user_actions_time ON public.user_actions USING btree (action_time);
 )   DROP INDEX public.idx_user_actions_time;
       public            postgres    false    228            S           1259    59348    idx_user_actions_user    INDEX     Q   CREATE INDEX idx_user_actions_user ON public.user_actions USING btree (user_id);
 )   DROP INDEX public.idx_user_actions_user;
       public            postgres    false    228            :           1259    59324    idx_users_email    INDEX     B   CREATE INDEX idx_users_email ON public.users USING btree (email);
 #   DROP INDEX public.idx_users_email;
       public            postgres    false    218            ;           1259    60058    idx_users_role    INDEX     @   CREATE INDEX idx_users_role ON public.users USING btree (role);
 "   DROP INDEX public.idx_users_role;
       public            postgres    false    218            �           2620    59731 2   received_products received_products_modify_trigger    TRIGGER     �   CREATE TRIGGER received_products_modify_trigger AFTER INSERT OR UPDATE ON public.received_products FOR EACH ROW EXECUTE FUNCTION public.update_quantity_store();
 K   DROP TRIGGER received_products_modify_trigger ON public.received_products;
       public          postgres    false    246    280            �           2620    60050    payments trg_audit_payments    TRIGGER     �   CREATE TRIGGER trg_audit_payments AFTER INSERT OR DELETE OR UPDATE ON public.payments FOR EACH ROW EXECUTE FUNCTION audit.fn_log_changes();
 4   DROP TRIGGER trg_audit_payments ON public.payments;
       public          postgres    false    248    283            �           2620    59675 +   pc_bookings update_booking_end_time_trigger    TRIGGER     �   CREATE TRIGGER update_booking_end_time_trigger AFTER INSERT OR DELETE OR UPDATE ON public.pc_bookings FOR EACH ROW EXECUTE FUNCTION public.update_booking_end_time();
 D   DROP TRIGGER update_booking_end_time_trigger ON public.pc_bookings;
       public          postgres    false    257    242            �           2620    59676 /   pc_bookings update_booking_total_amount_trigger    TRIGGER     �   CREATE TRIGGER update_booking_total_amount_trigger AFTER INSERT OR DELETE OR UPDATE ON public.pc_bookings FOR EACH ROW EXECUTE FUNCTION public.update_booking_total_amount();
 H   DROP TRIGGER update_booking_total_amount_trigger ON public.pc_bookings;
       public          postgres    false    275    242            �           2620    59351 1   orders_products update_order_total_amount_trigger    TRIGGER     �   CREATE TRIGGER update_order_total_amount_trigger AFTER INSERT OR DELETE OR UPDATE ON public.orders_products FOR EACH ROW EXECUTE FUNCTION public.update_order_total_amount();
 J   DROP TRIGGER update_order_total_amount_trigger ON public.orders_products;
       public          postgres    false    273    234            �           2606    76788    receipts FK_receipts    FK CONSTRAINT     �   ALTER TABLE ONLY public.receipts
    ADD CONSTRAINT "FK_receipts" FOREIGN KEY (payment_id) REFERENCES public.payments(id) NOT VALID;
 @   ALTER TABLE ONLY public.receipts DROP CONSTRAINT "FK_receipts";
       public          postgres    false    244    248    3463            �           2606    59076 (   blocked_users blocked_users_user_id_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public.blocked_users
    ADD CONSTRAINT blocked_users_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id);
 R   ALTER TABLE ONLY public.blocked_users DROP CONSTRAINT blocked_users_user_id_fkey;
       public          postgres    false    3393    218    226            �           2606    59211    bookings bookings_user_id_fkey    FK CONSTRAINT     }   ALTER TABLE ONLY public.bookings
    ADD CONSTRAINT bookings_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id);
 H   ALTER TABLE ONLY public.bookings DROP CONSTRAINT bookings_user_id_fkey;
       public          postgres    false    240    218    3393            �           2606    59043 "   employees employees_person_id_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public.employees
    ADD CONSTRAINT employees_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.persons(id);
 L   ALTER TABLE ONLY public.employees DROP CONSTRAINT employees_person_id_fkey;
       public          postgres    false    222    3385    216            �           2606    59048 $   employees employees_position_id_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public.employees
    ADD CONSTRAINT employees_position_id_fkey FOREIGN KEY (position_id) REFERENCES public.posts(id);
 N   ALTER TABLE ONLY public.employees DROP CONSTRAINT employees_position_id_fkey;
       public          postgres    false    222    220    3397            �           2606    59144 -   orders_products orders_products_order_id_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public.orders_products
    ADD CONSTRAINT orders_products_order_id_fkey FOREIGN KEY (order_id) REFERENCES public.orders(id);
 W   ALTER TABLE ONLY public.orders_products DROP CONSTRAINT orders_products_order_id_fkey;
       public          postgres    false    234    232    3426            �           2606    59139 /   orders_products orders_products_product_id_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public.orders_products
    ADD CONSTRAINT orders_products_product_id_fkey FOREIGN KEY (product_id) REFERENCES public.products(id);
 Y   ALTER TABLE ONLY public.orders_products DROP CONSTRAINT orders_products_product_id_fkey;
       public          postgres    false    234    230    3419            �           2606    59126    orders orders_user_id_fkey    FK CONSTRAINT     y   ALTER TABLE ONLY public.orders
    ADD CONSTRAINT orders_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id);
 D   ALTER TABLE ONLY public.orders DROP CONSTRAINT orders_user_id_fkey;
       public          postgres    false    218    3393    232            �           2606    59866    payments payments_user_id_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public.payments
    ADD CONSTRAINT payments_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;
 H   ALTER TABLE ONLY public.payments DROP CONSTRAINT payments_user_id_fkey;
       public          postgres    false    3393    218    248            �           2606    59224 '   pc_bookings pc_bookings_booking_id_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public.pc_bookings
    ADD CONSTRAINT pc_bookings_booking_id_fkey FOREIGN KEY (booking_id) REFERENCES public.bookings(id);
 Q   ALTER TABLE ONLY public.pc_bookings DROP CONSTRAINT pc_bookings_booking_id_fkey;
       public          postgres    false    242    240    3444            �           2606    59229 "   pc_bookings pc_bookings_pc_id_fkey    FK CONSTRAINT     |   ALTER TABLE ONLY public.pc_bookings
    ADD CONSTRAINT pc_bookings_pc_id_fkey FOREIGN KEY (pc_id) REFERENCES public.pc(id);
 L   ALTER TABLE ONLY public.pc_bookings DROP CONSTRAINT pc_bookings_pc_id_fkey;
       public          postgres    false    3442    238    242            �           2606    59186    pc pc_equipment_id_fkey    FK CONSTRAINT        ALTER TABLE ONLY public.pc
    ADD CONSTRAINT pc_equipment_id_fkey FOREIGN KEY (equipment_id) REFERENCES public.equipment(id);
 A   ALTER TABLE ONLY public.pc DROP CONSTRAINT pc_equipment_id_fkey;
       public          postgres    false    238    3438    236            �           2606    59725 4   received_products received_products_products_id_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public.received_products
    ADD CONSTRAINT received_products_products_id_fkey FOREIGN KEY (product_id) REFERENCES public.products(id) ON DELETE CASCADE;
 ^   ALTER TABLE ONLY public.received_products DROP CONSTRAINT received_products_products_id_fkey;
       public          postgres    false    246    3419    230            �           2606    59061    shifts shifts_employee_id_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public.shifts
    ADD CONSTRAINT shifts_employee_id_fkey FOREIGN KEY (employee_id) REFERENCES public.employees(id);
 H   ALTER TABLE ONLY public.shifts DROP CONSTRAINT shifts_employee_id_fkey;
       public          postgres    false    224    222    3401            �           2606    59100 &   user_actions user_actions_user_id_fkey    FK CONSTRAINT     �   ALTER TABLE ONLY public.user_actions
    ADD CONSTRAINT user_actions_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id);
 P   ALTER TABLE ONLY public.user_actions DROP CONSTRAINT user_actions_user_id_fkey;
       public          postgres    false    228    218    3393            �           2606    59008    users users_person_id_fkey    FK CONSTRAINT     }   ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_person_id_fkey FOREIGN KEY (person_id) REFERENCES public.persons(id);
 D   ALTER TABLE ONLY public.users DROP CONSTRAINT users_person_id_fkey;
       public          postgres    false    216    3385    218            S   �  x��WK�E]ל�/Щ���,ذ �lF²X�X6��l� q3Y�\��F���ή�����̢�{��/"���VJ ���O���_����ߒm�7�z����N����wÇ���Q��f)�\J�M`�lZ�7?������/��拯��a�n��ֿ����`�yB�2�$b�U�G�S���ʅ�����Vip��:{Z���Z|�����ㆩg땂d�If��`��Jz�W�Ϫ�TJ�1��lqV��@&��:S�ձ$�U���%jv_+�<���	�t���9q���#�cn�˲��<�&^y�=.S䆸.�L(���o�����ut[�.���֊�p)[C���<��w٩�ŵ����k/����!������{/�+�:�0���x^&�A%o��2�; <�Y�eQ��d�}]�U ��(�F�	͛��n�q��y��L��h7�%��uPh)Q�N��@sK�����~x��l��.^�a>Zp-�����@|2�N��`��l�lF�����}�}<�y���G?/��-�d�녝��'�J�<&G����A]xL�,6٧@#쒯n_߾|��Q��k���8������/����_2ܣY����~mc6���o�{�*X9�k���c�v�#zL���B<�����))AyQ=FN�ק$��*)M��(ά)�٤(m/���+����Շ&����D��c���V�b���F�ɧұ[r��M�(�ZM�\�����r�6�-M������W��.��am�ƕ���d%��9�^����y�ƕ"���ȅcN�2<m	k��
�`T���|�tk�5�x�)�t-�{�ֺ�\�.���)YKu���JJ��<;�#.�����1oN�5�}��@N�<����<�����99�����/W��1���:g�k_V�v�I�D�\�W,处"���mOa=i�����Rk�f�.��������      <      x������ � �      J   _  x���;�+7���U��Q�YK�
�*U��!p� �{�wt)�G�d�A�O��H��������O������4�d����_����??~}�~�~������8��xc�B�B�0o�\�i�ySU
��Ibt>��$�O��������3Iz~e>�F��?�7�y=�~�������7�2�r�Yt�y�?��C2J,Eడ��<�l��81�в��'!���!�W5A��ö�;'��
�.(7�!P
�&�_���a�;q�Ҥ��u�d�?&nL��s�'�=��[�7@�1L��/�1υ�Rќ�\��W�����`�A֚��6��O$)�	�v*5��v��};�:�>�A�/��K�j;��y��P���|�aʕt��EHS��/OsB���� C�`�YA�q1ɩ��$���-�4�˲apQ�o竣���j�~�V��������z��� ၣ��:���H�Q��W^���[�2�5).2p4�R��>�omռ4P�:�<�C㫚��b9Y�<��e�,X�7�̊�/%�ӎ+��$P���K׺|�!8S�`�8W�,\�'�«&�#ëj_Z�b��R��y)mÄئ?�bX��]*�C�Qؐ� z��|X:�"�,��*���R����J�h���]�C�� :Z ��M�v"�2�C���<���(dn(/�5��3�f������S�Ѧ��u \o=Zi��T� 1�̳Eڥ���/�Η�,��������2{�ױ�pv�']�Y�}�<�8�
u|"(���-g/�>���8֛U~�����������Jt1��_�v2̒�༽X풍�^�)L��U���I4��\T2�&�x��%;_IJ?l(���ܻ�<C
O�������}�8Jf×��n������d�v���U�S��җ2�������$�.렧����r��×�%�\ڷ�~-���K���D��RFr�YΆ�L#��u��FF�dq��O$�Q򐧯�P��'�7|y�O^�zkP�i/;_�F��	B(��o`}v��4�\}Co��������/ƙ>��)/�-y�1���po��q�ц��K�#9�|r���5TK�!���᫒��n�����B      8   /   x�3�475��4�0402�0�ӔӐ���50�52������� �N�      F   <  x�m��n�0E��W\��q��R��T�&S�Dcd�
���YQ��h4:��l��)4����j�c�Ƶ�#���0�|�Ҋ��;��nVoB�{개v�������(
���>g�l���C5T�����Kj)z�ævo<U�PH&b�G��?�M�B&�OXZ�����|zdZ���6�;�9=��a�(��ٱ���|��Q�ۻ��ڕ����wʲAp��T#�'����f�0M59S!+0�ۇ-ݎ�<P�kH8V��������3�#afk�CA�rzF�MSx?]�e]%+<ѱh��DWdζ�� ~9�\      B   �  x��WK��F\kN����l�%�q��"��>@n`y��a���Z�y�Hz��Y��d�X�uB�O�O�=���`5�`��I��P�������~�1~�ǯ������������|a_#�$�¢]�T� ��%)�Χ �@N 9:��� �#[�|�.�@��F8r(�B�
R P\k�	�o����׏���	��렖J�b�y��|��o~_���O������뇋<��A|P|/�]���s��\�P�Q��_[��k���ѲE��i�Ŷ:�LJ�1PB��q-����U{"+��I_fd����Fw�d�Q�`�f<cTO��,�[��.��(c�����V-b ᤐC�9�� �?�{�ճ4�i�w�����/�KY�[��u}J�B	���s��C�!�T�ܑ7K�h?oY���S�S9�~�F�i�����(���Ez��xPO�Vhg"M�eA<Pn���V�N'��d<�7����m �q�<��u|�8���!�`�Ll̃����M�Y"��1Ҁ��-K�4�+	�^|���e� ���B�X���AX��!H�5q*�ղ*m�r�pg�s��J$S�O�&��p�vvk�� V7��m`�$h,���}^$a�4�i'����GV�C�{�,�W~�����H̔��	 �%�>6fq���G%�.��L�հ��n��Ip��Dm�kf�1|�s�屺?J�zP�7�����������@<N��:�a�pa8 ;SQ�Zvs׍�E%5�S��n��-�����ssSV,>���1�(� :x���]BZ2ػ���!ڼ79&�E;�%�^�k���N��dX`��W�����?3x	��"�Rʃ�ϐ_֫� ��5����<A�т[ ��r�<��x��2P�]l���(����g%H�.��<�J!�u�_�]��ڎ�&FO%���� ��.���M]	�      D   �  x�-�۱!D��`n�K%��?�K��ԮgD��Y��tY�V��k�]W5�5����ջט=��Jq �q]�6Y��2Wքx/��!����� o�E�ؒ8t�\@e�@uڿP���]L]_7�|*J���K��Y�RE5������J���X�-}0JXt�ag�NrԘ�ސ����]�I&Z�+�&�Bw�_)l�>�f~�MvO�#�.�U�-��=��#&}Ŋt����9%.߈���_��Y�銨[g��1GS����yR۠��ng^�fS�vc{�����G1��vh�n���3w�1��!�����Q4���̎�O��mT���9�·۝�c��qi\@�v)�y���#��L����.�UVl�h	A��P�h	Cz�%���n�ww���É@�l	�� ��2��      R   �  x���[nd7��O��7P�����b�dy��fI�$�B����)���n�*� �hT}�$��I��>ʠ�������_~���o�lB�Wҫ�'����n��G>�~���&BT��Yk����ER�]��V��Nx�ʫ�EZӄg��^�Q��?_��\���W�y2��̿��3�ͳ���9�26��	$���9N���Æ��E�D�e߮��~��s���|�ǯ�λ2r���2d$��m��5�M#z�,>����X��<��t%5�Mq�8�Ō�zF�Me�vF�ù~Js��N��[ĩӝ����nv�;I1��%C�&���;-$/����
��gt
Jϫ�ʧ�o����n,~�Sv�S��I�z�mb��٩��b���Iaet"��*�+�UR��WI�*^%���Ds�# 9O�{��3>�����߀ǿo�}������Ziӻî�Ga�F>\��Ȧ��-}�pNی���\_����'o�´
ew:Z�ٵ�(TZ�.Q���Ad���݋5�u.�_�=tu�tնR-u���BVr�]�N�gw}e��D���}�\⃉J_{+��	�Ŏ<�:oAl�{b���t�?��<4*�Bx!EU��쌺�}���f�>Q/R�U��h��@i�V�%V$�{��aE�N�d9g��̎�~��8�m�V{F��#8��	#S�:����"�)s�-b��JQ���}}uq�ݬhm��������DHcuM�#�������	�C?}��JMøG�C#7����M;n�)��e �,%�5b�F�FOc̴�]�E���ס�ԋ�}��8���xMg.�s��`{��]DM�
���ZH*���f��F�j��"��HV��D�F�]4�,zu�Έ�ؚՑ��B��t#M��k����tZ�-w�C$!� ��ks�Ֆ+�
ƾr|mEf��a#�$"8���<k��u�&�7����":
�d���~����͌7��<ý&�]QI�m��;�=�6d� z�?�F2hJ+�yhV��Ȳ��XK��ۼ������w�ED!�����~ȸt�!|-��:z�$��-�&K`߉#�w�|���$�Q��W��E��=}D (�W�;e瀪�5bM��4�>�~+1��'4�ȾBk@�Q�rLI����t��b��泃��x�?���H C�2>���GF]3���+
�א�)�QlƤ�����#k�8��VV~(7�ƹ��J�7�����V�h@�`�R�/h!��3�\אBC�m&o6�lk�c�����7�}��rgM�	H�������2�!�y�����r�V�2�>���:F�_�Q���NB���!��eH�v5���c�;���}���96|��}��39B�#�����(TM�bqG�c����9jQI���Y��"K��o��1�B�nEwx�:�P��ǫ�%��N�Hu~~	8[)w�6*Jǟ���bɋ���;~��G�`_܇%wa�Y�)���&�8G�tW��y~_��9<��4��GB=o����f�G���=����^x=��A�<���ƌ�Xi�M;�1�:]=��$^���Յ=Y���2�a��UV&����$�X�9�E�b��+q -�����*.��,������1�0?��۫�<�Eދ�I�����Ϛ�G�\.���G�      H   �   x�}�1�0E��0(���=[%$&�.HH�.l��"N�`Ap��FD�T���'��fC�M�3�n�>=�5]��Y�5`����mD�R�w&�l�<�+݇CF�re���6+b�Bm<�� ��V֠m4)�S3+�+�VJ3V$��]w�Y����&o�IU<1:���)��J/*��Ҵ����SֱhԂEU��e�#�_l1�־�#g^      L   R  x�5�ɑ� D�(B�2��1-�q�v/>��s�a*��ϰ%9V��R�e�P��~Y ���eH��'�r����aKQ�!C4~x����M�m��T�`�����Ah��gq�Ʋs/#A:e9iw��9�K��wpA����Ж�$����uw$�c�60F�lsc��D�_�[��<z�����h��Iy��%H/	�ɞlp���`�`P�z��2Iv)��%�T��T6`Ï��C-�W��t�d��wJ�)�6����;��#|s�(�� ��t`P�<�3%���0�K��k��v|�j��	g3�����N�^6��]3��D��:~      2   w   x�3�0�¦.��0�¾�/�\lқ8��-,,�,-͹�8/̽��®;.6]��ya����/l���� �+� *6�0	h����J--�,��,,L�M��ߔ+F��� E5S�      6   6   x�+ ��1	Администратор	22000.00
\.


7h-      @   �  x���Mo�@�ϛ_�������ǖ"�����T9�65q�h툆S[�@J)�"j)��P���*���/��#f��
	��g�w�ٙyǌ�G�O�,;pFaЧ�C�0f�]���Fe᷍Jx����u؄OЅm��6��a�`s/]؅��Ir�L��.�\�SP�`%W�,����谅\�l�p�x�/P�p���O�-H�@���(�<��0��\�
�Ρ/�|��	�ȸ�wLRb$�]F�^���I[)����x�8��;p�Ԍi�ɨ&mݬsb�p���%M�Y��S!6���dT�If΃Y.|ӧ�G*+EZƞ�ӹibٮ�7�g��4N�F��X=,����)]�������W\��r\2��Nq/Y��V��s�¥���r�8�f0��3Q+�#�/�������C.jmj;&�Oݭ��HM+�*t�X�M�����:L#cm���<~X�P��_�C/�l��nX���H��2�樊�b�y���UQuP�L{�;�j4�I�b��j;��:h4��)\�١c����>p��+�v�e�r ���9$�ڱ7x���R"�M.��DD�,ØދW���I�rU�Q�'%;˚�E�sɨ�v9�Du�ƍ�^��� ���V�N�����&�#%x��:����SΆ~����s�biz�T�=R�&q&4~Pq�^�s��BEɧ      N   �  x��[�n���>�B7��s���]RmѠMc����.a�T)��j���-`m�^i��}5�P�y�����(�.wE�K�`C�r���w���ό�J47��	2F�h�h^Z��ƃ��(=8�I���;)��s��ݻ�������E�z�����i�mld���;��᰻ǯ� ��ϣ���+|���N��#x�+�9>H�#��`gt|�O�}Gҟ�?��ϳ�9��_�������	���[�Q>|�+uS�.8�h�$JÒ҃���8M�@����h�?=�b�o����d����������FX.$��R; �j����#�*;���{vZޜ�B��~=�����k��*���^|�}�}Q��p6˭t�������M��E��rПH���A�8?Ʌ��w���]�D�D�p9�����Qw���w4�Gu�`6��u������r�\#�4�� �7�����A�
來�-�;�$�̡��F�:�^�����!�^���*��U��4CO�z�r���(�h%�3�5z���)����Z�P߇�?���V����}Zj�����㿌���.�{���O��TR���B�C-�������d�O'�y���O�:;s��-�J�������a��o�+��w)�*]��z��ۣ	#6P(0*� 
ct�C�0��Di�L�M@�#x�\	x�HH�
e4(6�7����� +D�U�b̚�Y�(�������^���F�˲UI�����1d��'[�8Y|aNc
5g�w����;;ٿh�Sz���$�����:�5~F/�TByB5&-��C2�^z�lؽ��wz1XS��$�D9>�9�)o��t?~�~�����Ƿh�{>��e�\u�j�?��^g*[9/j�~��e?���Ǟ�q0Lc"J�^��3�ۓ���'%;�U��A�Ad�����3O��$��(M{��a�4����hh�g�����K���	Ȍ���寞'��/W�{L���y���\�מ�veٶ*;'�"}I��ﻟ0C�ҊsWb�>c�NX��t�s��8	sy7yP��
q$X����-ؘ�P�%����d5�f#/F~ �/�6AR�6`�:�i��m�3�T�p���q�g��(� 1
�2��T����ʯ�^�s��̒!U(��NoR���T�0��5�cj���mS�0��_y}#�������Û��M��pa��ٴ��f� �Rǭ F�+���%w�͉�%9<�
� <}�Ve@l�+:kh���|^���r`�V�����:ҙ�茪.i�TT5�=�Ug�����{0eՓ����������E0ha�.!]5�eO���V]ZTja�������e�����T0U�#����]�P���b��$�rL��6���V0Քia��pW��������r.�4�>5N4�ڄ��Sm8Ԡ�`փC��Oθ@XEn�P���Ν~n����]d-�p�;�k����;$��OCKSݪ���m���A��]��Wil�B �x���ɳu��6^�NN2�3�˘�=e@�`�g(rif���uF���t���9! �9�G�����>�s�V���
WS|�Z x�|��*�;�ϱْ�*�N�����t�6�U��e� A#@U������ba`>D��S	@	#�V�Ju�[)t��f6n�=��݉MI/��t�lF8�a�{*F�E	Ab���Ƣ��X4�L:�7X$�o��W����00G_E8�)IQ,A�-i����6�:��!�E�N�H.���YӺ����*'4����.����
ڞ���.wQ���h�	�A)L���)��wH��:�m�Rԙ�����&W�(^lXg&B5ә�KV|kC����'
��V��Z�ʵ�Ŵ�qn�8T��ꭖ�L!�W�.���u}e�[�:�?��-JҖi�e��!���!m��l`�����`o�3J�~�a����o��l,U5��`
s�}��}J&�A_�:������W��U�C���>�0�#5zت<���0lQ2�W_Sh��˙�*d�5D�/��JF:>�5�ݎ�k�7��ҵ9#ąwS��˰>���|*#�|ż�~���K؂���qVT��6�q3j���J�J�Q���ZEJ��J!���V����c�(I]�oWr�p���hB�z�ʊ��9�:RP��2$&h۲�b������;��5�N(آ^"G⁪00��DwDE���      P   k   x�U��0C�s3Eh'q�d����� ���d4S�P����
k���[ �'��r�T�Fc��/0��/���|���@G��s�%���b¯a�.��&=���1E�	���      :   �   x�m�э�0��Mi 0�Z��:�v/�$�����x����eL䴤4G��A�5�_�ŕ���^�@�dEu����\��t} er�ᣓ~o���8Ie�u[�L��Nև��f��k�Ȍ�;�
���:)|z�;�q�7<PP��~IG��a��TY��;� .7Y}����h�J6��+������+���?3��q��0b5�u�m���Abc      >   
  x���[�1E�ݫ�F�C��Z��@�W��dCH d�yG�n{˪�Z�`C�+��U�ħ���֗˧�������'Jg��=���S�v�r����$��� �!��i��QrC���|�|T�O���������kO!fe-���]�[.���ᝐ�`o��t���\���!�G�?��c:�[B���|;E�V�Q��p����8$��s��;�hߎcj����C"�n_�O�-s5-c��t`����/I��wx�XN�đ�*���V�g�
�VX��<=�t�"ӊ0���X��j��L��m-cu[K���2�n�9����]9�B���9p[P)}#����PD\�1���8n�X@��!FF3�w4;����� �!]"�^8���N��	��G�l_4�p	.��d�F7�`Y��j����j���C�x�@w݄X� �Vzܩ�J�SHiE�P�����H�H�d,��
����E	 �-Ɏ���/L.	x�]A��n��V3g�Q�nY��E�      4   �  x�e�Mr�0 ��8��B?�]���5���5�e�E���N�kt����iǞ�����g�ɮ�K?Wy#����#���)���u̾�ǧ�$ވ�����n�,��"r�]����.�@��NBU]u�����- !J-�:ܶ�6"R�
�;Y./pI(rSd�� K��N$n�ʀ�E��C5�Y4U&���J8a?�B�2K���w2��0lQ@1�:o�m0p�����������!ΥC�G,��m��,l<�
��Ko��n�@z��4����8�Qdn��d;�V'�>W�Z�)�J�d3������c�c�	������|���g��`�M�h�U&C�H���6X�����[���BZ�T{ů��'X��Q@8�� 3�#4�^ݛ�     